require 'json'
require 'thread'

END_CMD       = 'done'
ENCODING      = 'UTF-8'
LOG_SEVERITY  = :info
WAIT_TIMEOUT  = 30 #30 Seconds

#STDIN.sync   = true
#STDOUT.sync  = true
#STDERR.sync  = true

################################################################################

LogSeverity = {
  :fatal => 0,
  :error => 1,
  :warn => 2,
  :info => 3,
  :debug => 4,
  :trace => 5
}

def log(message, severity: :info, timestamp: Time.now, stack: '')
  return unless LogSeverity[severity] <= LogSeverity[LOG_SEVERITY]
  body = { :log => {
    :severity => severity.to_s,
    :message => message,
    :time => timestamp,
    :stackTrace => stack
  }}
  puts JSON.generate(body)
end

def return_result(result)
    body = { :result => { :data => result } }
    puts JSON.generate(body)
end

def write_error(message, timestamp: Time.now, location: '', stack: '', terminating: false)
  body = { :error => {
    :message => message,
    :time => timestamp,
    :location => location,
    :stackTrace => stack
  }}
  log(message, severity: :error, timestamp: timestamp, stack: stack)
  STDERR.puts JSON.generate(body)
  exit(1) if terminating
end

def return_entity(props)
    body = { :entity => props }
    puts JSON.generate(body)
end

################################################################################

def open_case(path)
  log "Opening case: #{path}"
  nuix_case = $utilities.case_factory.open(path)
  return nuix_case
end

def close_case(nuix_case)
  log "Closing case: #{nuix_case.location}"
  nuix_case.close
  return nuix_case.is_closed
end

def BinToHex(s)
  suffix = s.to_s.each_byte.map { |b| b.to_s(16).rjust(2, '0') }.join('').upcase
  return '0x' + suffix
end

################################################################################

log "Starting"

$utilities = utilities if defined? utilities

functions = {}

loop do

  log("reader: waiting for input", severity: :debug)

  input = STDIN.gets(chomp: true, encoding: ENCODING)

  log("reader: received input", severity: :debug)

  begin
    json = JSON.parse(input)
  rescue JSON::ParserError
    write_error("Could not parse input: #{input}")
    next
  end

  cmd = json['cmd']

  break if cmd.eql? END_CMD

  args = json['args']
  fdef = json['def']
  is_stream = json['isstream']

  #puts cmd
  #puts args.inspect
  #puts fdef

  unless fdef.nil?
    op = functions.key?(cmd) ? 'Replacing' : 'Adding new'
    log("#{op} function for '#{cmd}'", severity: :debug)
    functions[cmd] = {
      :accepts_stream => true,
      :fdef => eval(fdef)
    }
  end

  unless functions.key?(cmd)
    write_error("Function definition for '#{cmd}' not found", terminating: true)
  end

  if is_stream
    if !functions[cmd][:accepts_stream]
      write_error("The function '#{cmd}' does not support data streaming", terminating: true)
    end
    datastream = Queue.new
    if args.nil?
      args = { 'datastream' => datastream }
    else
      args['datastream'] = datastream
    end
    dataInput = Thread.new {
      datastream_end = nil
      loop do
        data_in = STDIN.gets(chomp: true, encoding: ENCODING)
        if datastream_end.nil?
          datastream_end = data_in
        elsif datastream_end.eql? data_in
          datastream.close
          datastream_end = nil
          break
        else
          datastream << data_in
        end
      end
    }
  end

  # TODO: rescue
  begin
    result = send functions[cmd][:fdef], args
    dataInput.join(WAIT_TIMEOUT) if is_stream
    return_result(result) # unless result.nil?
  rescue => ex
    write_error("Could not execute #{cmd}: #{ex}", stack: ex.backtrace.join("\n"), terminating: true)
  end

end

log "Finished"

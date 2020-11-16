require 'json'
require 'thread'

END_CMD       = 'done'
ENCODING      = 'UTF-8'
LOG_SEVERITY  = :info
INJECT_UTILS  = true
WAIT_TIMEOUT  = 90

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

def log(message, severity = :info, timestamp = Time.now, stack = '')
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

def write_error(message, timestamp = Time.now, location = '', stack = '')
  body = { :error => {
    :message => message,
    :time => timestamp,
    :location => location,
    :stackTrace => stack
  }}
  log(message, severity = :error, timestamp = timestamp, stack = stack)
  STDERR.puts JSON.generate(body)
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

queue = Queue.new

# Read from STDIN
reader = Thread.new do

  datastream = nil
  datastream_end = nil

  loop do

    log("reader: waiting for input", severity = :debug)

    inputWait = Thread.new { STDIN.gets(chomp: true, encoding: ENCODING) }

    input = nil
  
    if inputWait.join(WAIT_TIMEOUT).nil?
      write_error "Timed out waiting for input from STDIN"
      inputWait.kill
      input = "{\"cmd\":\"#{END_CMD}\"}\r\n"
    else
      input = inputWait.value
    end

    log("reader: received input", severity = :debug)

    if !datastream.nil? and !datastream.closed?

      if datastream_end.nil?
        datastream_end = input
      elsif datastream_end.eql? input
        datastream.close
        datastream_end = nil
      else
        datastream << input
      end

    else

      begin
        json = JSON.parse(input)
      rescue JSON::ParserError
        log("Could not parse input: #{input}", severity = :error)
        next
      end

      if json['isstream']
        datastream = Queue.new
        if json.key?('args')
          json['args']['datastream'] = datastream
        else
          json['args'] = { 'datastream' => datastream }
        end
      end

      queue << json

      break if json['cmd'].eql? END_CMD

    end

  end
end

################################################################################

$nuix_case = nil
$utilities = utilities if INJECT_UTILS

functions = {}

loop do

  json = queue.pop

  cmd = json['cmd']

  break if cmd.eql? END_CMD

  args = json['args']
  fdef = json['def']
  isstream = json['isstream']

  #puts cmd
  #puts args.inspect
  #puts fdef

  unless fdef.nil?
    op = functions.key?(cmd) ? 'Replacing' : 'Adding new'
    log("#{op} function for '#{cmd}'", severity = :debug)
    functions[cmd] = {
      :accepts_stream => fdef.include?('.pop'),
      :fdef => eval(fdef)
    }
  end

  unless functions.key?(cmd)
    err_msg = "Function definition for '#{cmd}' not found"
    log(err_msg, severity = :error)
    raise err_msg
  end

  if (isstream and !functions[cmd][:accepts_stream])
    err_msg = "The function '#{cmd}' does not support data streaming"
    log(err_msg, severity = :error)
    raise err_msg
  end

  # TODO: rescue
  result = send functions[cmd][:fdef], args
  return_result(result)

end

log "Finished"

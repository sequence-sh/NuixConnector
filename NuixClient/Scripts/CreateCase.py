import sys, getopt



def main(argv):
   path = ''
   name = ''
   description = ''
   investigator = ''
   try:
      opts, args = getopt.getopt(argv,"hi:o:",["path=","name=","description=","investigator="])
   except getopt.GetoptError:
      print 'test.py -i <inputfile> -o <outputfile>'
      sys.exit(2)
   for opt, arg in opts:
      if opt == '-h':
         print 'test.py -i <inputfile> -o <outputfile>'
         sys.exit()
      elif opt in ("-p", "--path"):
         inputfile = path
      elif opt in ("-n", "--name"):
         name = arg
      elif opt in ("-d", "--description"):
         description = arg
      elif opt in ("-i", "--investigator"):
         investigator = arg
       
         
   print "Creating Case"


    theCase = utilities.caseFactory.create(path, {
                                             'name' : name,
                                             'description' : description,
                                             'investigator' : investigator})

    print "Case Created"

    theCase.close()

if __name__ == "__main__":
   main(sys.argv[1:])



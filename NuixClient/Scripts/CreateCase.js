print(arguments);

var theCase = utilities.caseFactory.create('C:/Dev/Nuix/Cases/MyNewCase5');
try {
    theCase.name = 'New case';
    theCase.description = 'Description of the new case';
    theCase.investigator = 'My name';

    print('Created');

   
    print(theCase.name);
    print(theCase.description);
    print(theCase.investigator);

    // For audited licences the following will only work once the case has been verified.
    //print(theCase.rootItems.get(0).name);
    //print(theCase.search('kind:email AND name:18').get(0).name);
} finally {
    theCase.close();
}
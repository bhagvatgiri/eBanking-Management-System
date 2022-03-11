# eBM_System Project Instructions

==> Account information and Transacton Detail Table:

* Account information table Columns:

Username, Account Type(debit/savings/checking), Account Number(8 digit random number(but it should be 8 digit no more no less, if user does not add 8 digit it should show error)),Associated Card(Credit/Debit), Card Numbers(16 digit random number again required 16 digits)(cards should be only visible to users not admin)

* Transaction history Table columns:

Transaction ID(four digit random number), Transaction Type(Debit/credit), Date & time of transaction made., Amount.

==>Project Access and Configuration information:
	=>First start the .sln file in your visual studio and then go to connected services and go to connected settings and  configure your database. Once you configure your database and you go to edit dependency you will be able to get your connection string of the server. Copy that connection string and go to appsettings.json file in the repo and paste that connection string in the connection string parameter without changing anything else other than that.


 ==>Now on the left bottom of the visual studio you are able to packet manager console, click on it and type these two commands and follow the instructions:
  
1. Go to Tools -> NuGetPackage Manager -> Package Manager Conso

2. Command 1: add-migration "any comment that you want to add"

3. Command 2: update-database

=>And now start the project, it will run in  your browser(preferred firefox), if you do not have firefox installed you can change it in the settings.
=>Once the project start, I have already entered some entries in the database but if you decide to restart the database you can enter entries as an admin, because I have hardcoded the admin creds(In this place you will have too hardcode your credentials) in the code with every time code starts, admin gets assigned to the eBMS system and you will be able to login using Admin creds given in this document, if you have rebuild the database you can also add given users in the admin panel.

=>Also, I have implemented a lockout feature to so if you attempt to login with wrong creds more than 5 times it will lock you out for 20 mins.

=>Admin can add user and also add user’s transaction history and see their account information, but he/she cannot see user’s credit card information.

=>Admin Can add any number of transactions for user including his self. Also, User can add any number of transaction but he can only add one time Account information which admin cannot add it, he can just view it.

=>Also, any other user who wants to sign up to the system has to email admin with user email id and his password that he wants so to set it as.

=>Once ADMIN or USER clicks on transaction history he/she will be able to see different transaction ids in hyper link, only admin will be able to see user name and transaction id, but user will be able to see only transaction id nothing else. I have user ajax query string command for this.

=>I also tried applying SQL injection,  but it was not working as I used user role management.

=>Some different methods people Can try to do is
+ HTTP injection
+ XSS
+ SQL Injection
+ CSRF


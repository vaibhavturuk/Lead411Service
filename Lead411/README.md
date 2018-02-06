1) Must update connection string in web.config file:
Windows Authentication:
`<add name="AzureDBContext" connectionString="Data Source=<YOUR SERVER NAME>;Initial Catalog=Lead411;Integrated Security=True;MultipleActiveResultSets=True;" providerName="System.Data.SqlClient" />`

Server Authentication:
`<add name="AzureDBContext" connectionString="Data Source=<YOUR SERVER NAME>;Initial Catalog=Lead411;Username=<USERNAME>;Password=<PASSWORD>;MultipleActiveResultSets=True;" providerName="System.Data.SqlClient" />`

------------------------------------------------------------

2) To generate database, After successful restoring of Nuget Packages 
Run unit test case
Or
Run application by F5 key and hit http://localhost:55555/api/AdminPanel url.

[Becase Database will only get generated on the creation of dbcontext object]
# DotNetBlog
A Tiny Blog Written in Asp.Net Core

How to Build and Run:

*   Clone the repository
*   Restore the dependencies

    ```
    dotnet restore
    ```
*   Build admin portal

    ```
    cd src/DotNetBlog.Admin
    npm install
    webpack
    ```

*   Edit connection string

	Open 'src/DotNetBlog.Web/App_Data/config.json' to specify you own connection string.

    ```
    {
        "database": "sqlite",
        "connectionString": "DataSource=App_Data/blog.db"
        //"database": "sqlserver",
        //"connectionString": "server=.\\SqlServer2008;database=DotNetBlog;uid=sa;pwd=123456;"
    }
    ```

    DotNetBlog supports two kinds of database. You can set "database" to "sqite" or "sqlserver"
*   Run the project

    ```
    dotnet run
    ```
*   Initialize the blog

    You can access 'http://{YourBlogAddress}/install' to initialize the blog.

Setup & Run
1. Open SQL Server Management Studio (SSMS) or LocalDB `(localdb)\MSSQLLocalDB`.
2. Execute `Database/SaigonBite_Schema_And_Seed.sql` to create `SaigonBiteDB` and seed sample hubs, brands, menu items, vouchers, and test accounts.
3. Open `SaigonBite.sln` in Visual Studio 2022 (.NET 8 Desktop Development workload).
4. Run `dotnet test` to verify all 33 unit tests pass.
5. Set `SaigonBite.UI` as Startup Project and press `F5` (or build Release to `SaigonBite.UI\bin\Release\net8.0-windows\SaigonBite.UI.exe`).
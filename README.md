# 🛒 Shop Management System (ADO.NET Project)

A simple Windows Forms application built using **ADO.NET** and **SQL Server** for managing shop customers, products, members, and sales records.

## 📋 Features

- Add, update, delete, and view products
- Maintain customer and membership records
- Handle sales transactions with total price calculations
- Image storage for customer and member profiles
- Stored procedure-based operations for better performance and security

## 🗂️ Project Structure

- **ADO.NET Windows Forms UI**
- **SQL Server** as the database
- **Stored Procedure (`sp_Product`)** for product management
- Uses `App.config` for database connection string

---

## 🧱 Database: `ShopManagementDB`

### 🧾 Tables

- `Customer`: Holds customer data (name, purchase date, member status, photo)
- `Members`: Stores member information (unique phone and email)
- `MemberType`: Defines membership types
- `Products`: Product inventory (name, Mfg date, Expiry date, type)
- `Sales`: Sales records with quantity, price, and total

### 🔗 Relationships

- `Members.MemberTypeId` → `MemberType.Id`
- `Sales.CustomerId` → `Customer.CustomerId`
- `Sales.ProductId` → `Products.ProductId`

---

## ⚙️ Stored Procedure: `sp_Product`

A single procedure handles:

| Action Type      | Description                    |
|------------------|--------------------------------|
| `SaveData`       | Insert or update a product     |
| `DeleteData`     | Delete a product by ID         |
| `ShowAllData`    | Retrieve all products          |
| `ShowAllDataById`| Get details of a product by ID |

---

## 🧾 App Configuration

Ensure your connection string in `App.config` points to the correct SQL Server instance:

```xml
<connectionStrings>
  <add name="db" connectionString="Server=DESKTOP-FRGGIBC\SQLEXPRESS;Database=ShopManagementDB;Integrated Security=True;" providerName="System.Data.SqlClient"/>
</connectionStrings>

Also change connection string in Product.cs file here 
string conString = "Server=DESKTOP-FRGGIBC\\SQLEXPRESS;Database=ShopManagementDB;Integrated Security=True";


🛠️ How to Run
🧩 Create the Database
Execute the provided SQL script to create ShopManagementDB and all its tables, constraints, and stored procedures.

🧑‍💻 Open the Project
Load the solution into Visual Studio.

⚙️ Configure Connection String
Check App.config for the correct SQL Server name.

▶️ Run the App
Build and run the app from Visual Studio. Perform operations like adding products, managing customers, and viewing sales.

📸 Notes
Customer.Picture and Members.Picture support image uploads and display.

.vs/ and other temporary files are excluded via .gitignore.

📦 Technologies Used
C#, Windows Forms

ADO.NET for database operations

SQL Server with stored procedures

Git for version control

✅ To Do
 Add reporting feature

 Implement user login system

 Export sales data to Excel or PDF

📄 License
This project is for learning and academic purposes. Feel free to fork and customize it!

🙋‍♀️ Author
Jasmin
Student Developer

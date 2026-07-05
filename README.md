# 🔍 Crime Analysis & Safety Dashboard

> A professional desktop application for law enforcement agencies to analyze crime data, track suspects, manage officers, and generate reports through an interactive dashboard.

![Platform](https://img.shields.io/badge/Platform-Windows-0078D6?style=for-the-badge&logo=windows)
![Language](https://img.shields.io/badge/C%23-.NET%2010-512BD4?style=for-the-badge&logo=dotnet)
![Database](https://img.shields.io/badge/SQL%20Server-2025-CC2927?style=for-the-badge&logo=microsoftsqlserver)
![UI](https://img.shields.io/badge/UI-WPF-68217A?style=for-the-badge)

---

## 📸 Screenshots

<table>
  <tr>
    <td align="center"><b>Login Window</b></td>
    <td align="center"><b>Main Dashboard</b></td>
  </tr>
  <tr>
    <td><img src="screenshots/login.png" width="400"/></td>
    <td><img src="screenshots/dashboard.png" width="400"/></td>
  </tr>
  <tr>
    <td align="center"><b>Crimes Management</b></td>
    <td align="center"><b>PDF Export</b></td>
  </tr>
  <tr>
    <td><img src="screenshots/crimes.png" width="400"/></td>
    <td><img src="screenshots/pdf.png" width="400"/></td>
  </tr>
</table>

> 📌 Add your screenshots to a `screenshots/` folder in the root directory

---

## ✨ Features

### 🔐 Security & Authentication
- Secure login system with username and password
- User registration with automatic **User** role assignment
- Two roles: **Admin** (full access) and **User** (view only)
- CRUD buttons automatically hidden for normal users

### 📊 Interactive Dashboard
- **4 Stat Cards** — Total Crimes, Cases Solved, Suspects, Officers
- **Alert Banner** — Highlights high-risk areas exceeding crime threshold
- **Bar Chart** — Crime count per area
- **Pie Chart** — Crime type breakdown
- **Line Chart** — Monthly crime trend over 12 months
- **Advanced Filters** — City, Crime Type, Severity, Date Range

### 🚨 Crime Management (CRUD)
- View all crime records in a searchable DataGrid
- Add new crime with type, area, officer, date, severity
- Edit existing crime records
- Delete crime with confirmation dialog
- Filter by city, status, and severity

### 👥 Suspects Tracking
- Link suspects to specific crime cases
- Track status: Wanted, Arrested, Released
- Add and delete suspect records

### 👮 Officers Management (CRUD)
- Manage law enforcement officers
- Track badge numbers, ranks, and assignments
- Full Add, Edit, Delete operations

### 📄 PDF Export
- Generate professional 2-page PDF reports
- Page 1: Summary statistics and high-risk areas
- Page 2: Detailed crimes list
- Reports logged to database for audit trail

### 🌙 Dark Theme UI
- Professional dark theme throughout
- Red accent colors matching law enforcement aesthetic
- Consistent design across all windows

---

## 🛠️ Tech Stack

| Technology | Version | Purpose |
|---|---|---|
| C# | .NET 10 | Primary programming language |
| WPF | .NET 10 | Desktop UI framework |
| SQL Server Express | 2025 | Relational database |
| SSMS | Latest | Database management |
| LiveChartsCore.SkiaSharpView.WPF | 2.0.4 | Interactive charts |
| PDFSharp | 6.2.4 | PDF report generation |
| Microsoft.Data.SqlClient | 7.0.1 | SQL Server connectivity |

---

## 🗄️ Database Schema

**Database Name:** `CrimeAnalysis`

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│    Users    │     │    Areas    │     │ CrimeTypes  │
│─────────────│     │─────────────│     │─────────────│
│ UserId (PK) │     │ AreaId (PK) │     │ TypeId (PK) │
│ Username    │     │ AreaName    │     │ TypeName    │
│ Password    │     │ City        │     │ Category    │
│ Role        │     └─────────────┘     └─────────────┘
│ FullName    │              │                  │
└─────────────┘              │                  │
                             ▼                  ▼
┌─────────────┐     ┌──────────────────────────────────┐
│  Officers   │     │             Crimes               │
│─────────────│     │──────────────────────────────────│
│OfficerId(PK)│────▶│ CrimeId (PK)                     │
│ FullName    │     │ TypeId   (FK → CrimeTypes)       │
│ BadgeNumber │     │ AreaId   (FK → Areas)            │
│ Rank        │     │ OfficerId(FK → Officers)         │
│ City        │     │ CrimeDate                        │
│ Phone       │     │ Severity (Low/Medium/High)       │
└─────────────┘     │ Status  (Open/Closed/Investing)  │
                    └──────────────────────────────────┘
                              │
                              ▼
                    ┌─────────────────┐     ┌─────────────┐
                    │    Suspects     │     │   Reports   │
                    │─────────────────│     │─────────────│
                    │ SuspectId (PK)  │     │ ReportId(PK)│
                    │ CrimeId (FK)    │     │ GeneratedBy │
                    │ FullName        │     │ GeneratedAt │
                    │ Age             │     │ ReportType  │
                    │ Gender          │     │ FilePath    │
                    │ Status          │     └─────────────┘
                    └─────────────────┘
```

### Table Summary

| # | Table | Description |
|---|---|---|
| 1 | **Users** | Login credentials and role-based access control |
| 2 | **Areas** | Geographic areas in Abbottabad, Haripur, Mansehra |
| 3 | **CrimeTypes** | Crime categories (Robbery, Assault, Burglary, etc.) |
| 4 | **Officers** | Law enforcement officer details and assignments |
| 5 | **Crimes** | Main crime incident records (links 3 tables via FK) |
| 6 | **Suspects** | Suspects linked to crime cases |
| 7 | **Reports** | Audit log of all generated PDF reports |

---

## 🚀 Getting Started

### Prerequisites

- [Visual Studio 2022](https://visualstudio.microsoft.com/) with .NET Desktop Development workload
- [SQL Server 2025 Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)
- .NET 10 SDK

### Installation

**1. Clone the repository**
```bash
git clone https://github.com/yourusername/crime-analysis-dashboard.git
cd crime-analysis-dashboard
```

**2. Set up the database**
```
- Open SQL Server Management Studio (SSMS)
- Connect using Windows Authentication
- Open a New Query window
- Open and run: Database/database_setup.sql
- This creates all 7 tables and inserts sample data
```

**3. Update the connection string**

Open `Crime_analysis/Services/DatabaseHelper.cs` and update:
```csharp
private readonly string _connectionString =
    "Server=YOUR_PC_NAME\\SQLEXPRESS;" +
    "Database=CrimeAnalysis;" +
    "Trusted_Connection=True;" +
    "TrustServerCertificate=True;";
```
Replace `YOUR_PC_NAME` with your actual computer name.

**4. Install NuGet packages**
```
Open Package Manager Console in Visual Studio and run:

Install-Package Microsoft.Data.SqlClient
Install-Package LiveChartsCore.SkiaSharpView.WPF
Install-Package PdfSharp
```

**5. Build and run**
```
- Open Crime_analysis.slnx in Visual Studio 2022
- Press Ctrl+Shift+B to build
- Press F5 to run
```

---

## 🔑 Default Login Credentials

| Role | Username | Password | Access |
|---|---|---|---|
| **Admin** | `admin` | `admin123` | Full access (view + add + edit + delete) |
| **User** | `user1` | `user123` | Read only (view and filter only) |
| **User** | `user2` | `user123` | Read only (view and filter only) |
| **User** | `Hanzi` | `Hanzi01` | Read only (view and filter only) |

> 💡 You can also register a new account from the login screen. New accounts are always created as **User** role.

---

## 📁 Project Structure

```
Crime_analysis/
│
├── 📁 Models/                  # Data model classes
│   ├── User.cs
│   ├── Crime.cs
│   ├── Area.cs
│   ├── Officer.cs
│   ├── Suspect.cs
│   └── AreaAlert.cs
│
├── 📁 Services/                # Business logic & data access
│   ├── DatabaseHelper.cs       # All SQL queries
│   ├── AuthService.cs          # Login & session management
│   └── FontResolver.cs         # PDF font handling
│
├── 📁 Views/                   # WPF window files
│   ├── LoginWindow.xaml        # Authentication window
│   ├── RegisterWindow.xaml     # New user registration
│   ├── CrimeFormWindow.xaml    # Add/Edit crime form
│   ├── OfficerFormWindow.xaml  # Add/Edit officer form
│   └── SuspectFormWindow.xaml  # Add suspect form
│
├── 📁 Styles/                  # XAML styles & themes
│   └── AppStyles.xaml          # Global dark theme styles
│
├── 📁 SQL/                     # Database scripts
│   └── database_setup.sql      # Full DB setup + sample data
│
├── MainWindow.xaml             # Main dashboard
├── MainWindow.xaml.cs          # Dashboard logic
├── App.xaml                    # Application entry point
└── App.xaml.cs
```

---

## 📊 Sample Data Included

The database script inserts:

- ✅ **4 users** (1 admin + 3 normal users)
- ✅ **9 areas** across Abbottabad, Haripur, and Mansehra
- ✅ **6 crime types** (Robbery, Assault, Burglary, Car Theft, Fraud, Kidnapping)
- ✅ **6 officers** with different ranks
- ✅ **500 crime records** with random dates, types, and severities
- ✅ **200 suspects** linked to crime cases

---

## 👥 Team Members

| Name | Roll Number | Role |
|---|---|---|
| [Hanzalla Rafaq] | Developer |
| [Ahmwd Raza] | Developer |

---

## 🎓 Course Information

```
Course    : Advanced Programming (C# .NET – Visual Studio)
Project   : Data-Driven Desktop Application
Date      : June 2026
```

---

## 📝 License

This project was developed as a university course project.
Feel free to use it as a reference for learning purposes.

---

<div align="center">
  <p>Built with ❤️ using C# and WPF</p>
  <p>⭐ Star this repo if you found it helpful!</p>
</div>

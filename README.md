# GD1 - Grand Auto Depot One (Backend API)

This repository contains the backend architecture and RESTful API powering the GD1 platform. It handles user authentication, vehicle management, property partnerships, booking logic, and role-based authorization for the entire ecosystem.

**[Looking for the Frontend UI Repository? Click Here](https://github.com/arunjosf/GD1-Frontend-)** 

## Live API
The backend API is deployed and running live on Render:
** [https://gd1-grand-auto-depot-one-9ms1.onrender.com](https://gd1-grand-auto-depot-one-9ms1.onrender.com)**

## Tech Stack
- **Framework:** .NET Core / ASP.NET (C#)
- **Database:** Entity Framework Core
- **Authentication:** JWT (JSON Web Tokens)
- **Deployment:** Render

## Key Features
- **Secure Authentication:** Robust JWT-based authentication system with Role-Based Access Control (Admins, Garage Owners, Vehicle Owners).
- **Property Management:** Endpoints for garage owners to register properties, manage slots, and update facility amenities.
- **Vehicle Storage Logic:** Complete backend pipeline for tracking when a vehicle is 'Idle' vs 'Stored', generating active booking IDs.
- **AI Integration Support:** Backend endpoints configured to support frontend chatbot requests and analytics.

## Local Setup

1. **Clone the repository:**
   ```bash
   git clone https://github.com/arunjosf/GD1-Grand-Auto-Depot-One-
   cd GD1-Grand-Auto-Depot-One-
   ```

2. **Configure Database Connection:**
   Update your `appsettings.json` or `appsettings.Development.json` with your local SQL database connection string.

3. **Run Entity Framework Migrations:**
   ```bash
   dotnet ef database update
   ```

4. **Run the Server:**
   ```bash
   dotnet run
   ```

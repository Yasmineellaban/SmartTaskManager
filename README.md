# SmartTask Manager

> A modern task management system built with ASP.NET Core MVC, SQL Server, ASP.NET Core Identity, and Gemini AI.

SmartTask Manager is a full-featured task management application designed to help users organize their work, manage priorities, track deadlines, and interact with an AI-powered assistant.

The project combines a clean SaaS-style interface with practical backend architecture and AI capabilities, including an **AI Assistant** and an **AI Agent capable of calling application functions**.

---

## ✨ Features

### 📋 Task Management

* Create, edit, view, and delete tasks
* Task descriptions
* Task priorities
* Task statuses
* Due dates
* Completion tracking
* Overdue task detection
* Task details and metadata

### 🗂️ Category Management

* Create and manage custom categories
* Assign tasks to categories
* View the number of tasks in each category
* Prevent deleting categories that contain tasks

### 💬 Task Comments

* Add comments to tasks
* Delete your own comments
* Track comment activity inside task details

### 📅 Calendar & Planning

* View scheduled tasks
* Upcoming task overview
* Completed task overview
* Due date and time display
* Quick navigation to task details

### 📊 Dashboard

* Total tasks overview
* Pending tasks
* In-progress tasks
* Completed tasks
* Completion rate
* Overdue tasks
* Recent tasks

---

## 🤖 AI Features

One of the main goals of SmartTask Manager is combining traditional task management with AI-powered interaction.

### AI Assistant

The application includes an AI Assistant powered by **Google Gemini**.

Users can ask questions using natural language and receive AI-generated responses.

Features include:

* Natural language interaction
* Streaming responses
* Markdown rendering
* Code block formatting
* AI-powered productivity assistance
* Real-time response display

### AI Agent

SmartTask Manager also includes an AI Agent capable of interacting with the application's backend through **function calling**.

The agent can:

* Understand user requests
* Retrieve available categories
* Select the correct category
* Create tasks automatically
* Handle task priority
* Handle descriptions
* Handle due dates
* Execute multiple tool calls when required

Example:

> "Create a high priority task called Prepare presentation in the Work category."

The agent can determine that it needs to:

1. Retrieve the user's categories
2. Find the correct category
3. Extract the task information
4. Call the `create_task` function
5. Create the task in the database
6. Return the result to the user

This creates a practical **AI → Tool → Application → Database** workflow.

---

## 🧠 AI Agent Architecture

The AI Agent uses function calling to connect Gemini with application functionality.

```text
User
  │
  ▼
AI Agent
  │
  ├── get_categories
  │        │
  │        ▼
  │   SQL Server
  │
  └── create_task
           │
           ▼
      Task Database
           │
           ▼
       Agent Result
           │
           ▼
          User
```

The agent is designed to avoid guessing category IDs.

For example, when a user specifies a category by name, the agent first retrieves the user's categories and then uses the correct category ID when creating the task.

---

## 🛠️ Technologies

### Backend

* C#
* ASP.NET Core MVC
* ASP.NET Core Identity
* Entity Framework Core
* LINQ
* SQL Server
* REST-style MVC architecture

### AI

* Google Gemini API
* Gemini Function Calling
* AI Agent workflows
* Streaming AI responses
* JSON-based tool communication

### Frontend

* Razor Views
* HTML5
* CSS3
* Bootstrap
* Bootstrap Icons
* JavaScript
* Markdown rendering

### Development Tools

* Visual Studio
* SQL Server
* Git
* GitHub

---

## 🏗️ Architecture

The application follows a structured ASP.NET Core MVC architecture.

```text
SmartTaskManager
│
├── Controllers
│   ├── HomeController
│   ├── TasksController
│   ├── CategoriesController
│   ├── CommentsController
│   ├── CalendarController
│   ├── AIController
│   └── AgentController
│
├── Models
│   ├── ApplicationUser
│   ├── TaskItem
│   ├── Category
│   ├── TaskComment
│   ├── TaskPriority
│   └── TaskStatus
│
├── Data
│   └── ApplicationDbContext
│
├── Services
│   ├── GeminiService
│   └── Agent
│       ├── AgentService
│       ├── GetCategoriesTool
│       └── CreateTaskTool
│
├── Views
│   ├── Dashboard
│   ├── Tasks
│   ├── Categories
│   ├── Calendar
│   ├── AI
│   └── Agent
│
└── Areas
    └── Identity
```

---

## 🔐 Authentication & Authorization

The application uses **ASP.NET Core Identity** for account management and authentication.

Implemented features include:

* User registration
* Login
* Logout
* Password reset
* Email confirmation
* Two-factor authentication
* Recovery codes
* Account lockout
* External login support
* Personal account management

Users can only access and manage their own tasks, categories, and comments.

---

## 🗄️ Database

SmartTask Manager uses **SQL Server** with **Entity Framework Core**.

Main entities:

```text
ApplicationUser
      │
      ├───────────────┐
      ▼               ▼
  Categories       TaskItems
                      │
                      ▼
                 TaskComments
```

### Main Relationships

* A user can have multiple categories
* A category can contain multiple tasks
* A task belongs to one category
* A task can have multiple comments
* Comments belong to both a task and a user

Entity Framework Core is used for:

* Database mapping
* Relationships
* Migrations
* CRUD operations
* LINQ queries

---

## 🎨 UI & Design

The application uses a modern SaaS-inspired interface with:

* Premium green color system
* Responsive layouts
* Glassmorphism elements
* Floating cards
* Smooth hover interactions
* Consistent spacing and typography
* Responsive mobile layouts
* AI-focused visual elements

The goal was to create an interface that feels like a modern productivity platform rather than a default ASP.NET application.

---

## 🚀 Getting Started

### Prerequisites

Make sure you have:

* .NET SDK
* Visual Studio
* SQL Server
* Git
* A Google Gemini API key

### 1. Clone the repository

```bash
git clone https://github.com/YOUR_USERNAME/SmartTaskManager.git
```

### 2. Navigate to the project

```bash
cd SmartTaskManager
```

### 3. Configure the database

Update the connection string in your configuration if needed:

```text
Server=.;Database=SmartTaskManagerDb;Trusted_Connection=True;TrustServerCertificate=True
```

### 4. Configure Gemini API

The Gemini API key should **not** be committed to GitHub.

Use ASP.NET Core User Secrets:

```bash
dotnet user-secrets init
```

Then configure your API key using your local development environment.

### 5. Apply migrations

```bash
dotnet ef database update
```

### 6. Run the application

```bash
dotnet run
```

Or run the project directly from Visual Studio.

---

## 🔒 Security Notes

Sensitive configuration values should never be committed to the repository.

The project uses **ASP.NET Core User Secrets** for development API credentials.

Before deploying the application, configure production secrets through a secure environment or secret-management solution.

---

## 📌 Project Highlights

SmartTask Manager demonstrates practical experience with:

* ASP.NET Core MVC
* Entity Framework Core
* SQL Server
* Authentication & Authorization
* CRUD operations
* Database relationships
* LINQ
* AI API integration
* Gemini Function Calling
* AI Agent workflows
* Streaming AI responses
* Responsive UI design
* Git & GitHub

---

## 🔮 Future Improvements

Possible future extensions include:

* AI-powered task recommendations
* Automatic task prioritization
* RAG-based productivity knowledge
* AI-generated daily schedules
* Notifications and reminders
* Advanced analytics
* Docker deployment
* Automated testing
* Production deployment

---

## 👩‍💻 Author

**Yasmine El-laban**

Management Information Systems Student
Backend .NET Developer — DEPI

Interested in:

* Backend Development
* ASP.NET Core
* AI Agents
* Generative AI
* Automation
* Software Engineering

---

## ⭐ Project

If you find this project useful or interesting, feel free to explore the repository and follow the development journey.

**Built with C#, ASP.NET Core, SQL Server, and Gemini AI.**

# Complete System Analysis and Backend Development Prompt

You are a Senior Solution Architect, Senior .NET 10 Developer, Database Architect, Security Expert, and DevOps Engineer.

Your task is to thoroughly analyze the entire Vue.js frontend application that has been provided to you.

Do not make assumptions.

You must inspect and understand every file in the project.

---

## Phase 1 – Full System Analysis

Analyze all Vue.js project files:

- Components
- Views/Pages
- Layouts
- Routes
- Store (Pinia/Vuex)
- Services / API layers
- Utilities
- Forms and validations
- Tables and grids
- Dashboards
- Authentication screens
- Master setup modules
- Transaction modules
- Reports
- Workflows

For every screen identify:
- Purpose
- Business Rules
- Data Requirements
- Required Database Tables
- Required API Endpoints
- Validation Rules
- Workflow Requirements

---

## Phase 2 – System Documentation

Generate:

### Executive Summary
- System Purpose
- Business Objectives
- Users

### Functional Analysis
For every page:
- Fields
- Actions
- Logic
- Dependencies

---

## Phase 3 – Database Design (MS SQL Server)

Design complete database:

Include:
- Users / Roles / Permissions
- Audit Logs
- Master Tables
- Transaction Tables
- Workflow Tables
- Notification Tables

---

## Phase 4 – Backend Architecture (.NET 10 Web API)

Use:

- Clean Architecture
- Repository Pattern
- Service Layer
- Entity Framework Core
- JWT Authentication
- SignalR
- AutoMapper
- FluentValidation
- Serilog

Structure:

/src
  API
  Application
  Domain
  Infrastructure

---

## Phase 5 – Authentication & Authorization

Implement:
- JWT Login/Logout
- Refresh Tokens
- Role-Based Access Control
- Permission-based security

---

## Phase 6 – API Development

For each frontend module create:

- GET
- POST
- PUT
- DELETE
- SEARCH
- FILTER
- PAGINATION

---

## Phase 7 – Integration Mapping

Map Vue.js frontend screens to backend APIs.

Ensure:
- No missing endpoints
- Full coverage of UI functionality

---

## Phase 8 – Deliverables

Generate:
- SQL Database Script
- .NET 10 Web API Project
- Controllers / Services / Repositories
- DTOs
- Authentication Module
- Logging & Audit System
- API Documentation

---

## Critical Rules

- Analyze every file carefully
- Do not assume anything
- Follow enterprise architecture standards
- Ensure production-ready code

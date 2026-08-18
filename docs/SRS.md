## for

# Vehicle Service Center Job, Parts & Maintenance Tracking System

**Module:** SE3022 - Case Study Project

**Year:** 3 - Semester 1

**Academic Year:** 2026

**Group:** Group 20 | IT24103467, IT2410342, IT24103441, IT24103453

**Version:** 1.0

---

## Revision History

| Version | Date | Description |
| --- | --- | --- |
| 1.0 | August 2026 | Initial version of the SRS |

---

## Table of Contents

1. Introduction
    
    1.1 Purpose
    
    1.2 Document Conventions
    
    1.3 Intended Audience
    
    1.4 Product Scope
    
    1.5 References
    
2. Overall Description
    
    2.1 Product Perspective
    
    2.2 Product Functions
    
    2.3 User Classes and Characteristics
    
    2.4 Operating Environment
    
    2.5 Design and Implementation Constraints
    
    2.6 Assumptions and Dependencies
    
3. External Interface Requirements
    
    3.1 User Interfaces
    
    3.2 Hardware Interfaces
    
    3.3 Software Interfaces
    
    3.4 Communication Interfaces
    
4. System Features
    
    4.1 User Management
    
    4.2 Customer and Vehicle Management
    
    4.3 Service Booking
    
    4.4 Vehicle Check In
    
    4.5 Inspection and Job Management
    
    4.6 Spare Parts Inventory Management
    
    4.7 Billing and Payment Management
    
    4.8 Notification Management
    
    4.9 Vehicle Service History
    
    4.10 Dashboard and Reporting
    
5. Non-Functional Requirements
    
    5.1 Performance
    
    5.2 Security
    
    5.3 Reliability
    
    5.4 Usability
    
    5.5 Maintainability
    
    5.6 Scalability
    
    5.7 Testing
    
6. Other Requirements
    
    6.1 Database Requirements
    
    6.2 Microservice Requirements
    
    6.3 Kafka Communication
    
    6.4 CI/CD
    

Appendix A - Glossary

Appendix B - System Models

Appendix C - Future Improvements

---

# 1. Introduction

## 1.1 Purpose

The purpose of this document is to describe the requirements of the **Vehicle Service Center Job, Parts & Maintenance Tracking System**.

The system is designed to help a vehicle service center manage its main activities digitally. It covers the process from service booking and vehicle check in to inspection, repair work, spare part usage, billing, customer notifications, and service history.

This document explains what the system should do, who will use it, and the main technical requirements that will be followed during development.

## 1.2 Document Conventions

Functional requirements are grouped according to the main features of the system.

Each requirement is given an ID such as:

- FR-UM: User Management
- FR-CV: Customer and Vehicle Management
- FR-BKG: Booking
- FR-CHK: Vehicle Check In
- FR-JOB: Job Management
- FR-INV: Inventory
- FR-BIL: Billing
- FR-NOT: Notifications
- FR-HIS: Service History
- FR-REP: Reports

This makes the requirements easier to identify when creating the product backlog and test cases.

## 1.3 Intended Audience

This document is mainly intended for:

- Project team members
- Lecturers and evaluators
- Developers
- QA members
- DevOps members
- Business Analysts
- The selected client or stakeholder

## 1.4 Product Scope

The **Vehicle Service Center Job, Parts & Maintenance Tracking System** is a web based system designed to manage vehicle servicing activities in a vehicle service center.

Many vehicle service centers still depend on paperwork, phone calls, and manual records to manage customers, vehicles, repairs, spare parts, and payments. This can make it difficult to track the progress of a vehicle, manage stock, or find previous service records.

The proposed system will bring these activities into one platform.

The main areas of the system are:

- Customer management
- Vehicle management
- Service bookings
- Vehicle check in
- Vehicle inspection
- Job cards
- Mechanic assignments
- Repair progress
- Spare-part inventory
- Billing
- Payments
- Notifications
- Vehicle service history
- Reports and dashboards

The system will also use microservices and Apache Kafka so that different parts of the system can communicate with each other.

## 1.5 References

The following materials were used as references when planning the project:

- SE3022 Case Study Project lecture materials
- Kafka and .NET Microservices workshop material
- Microservice Communication Architecture guide
- Lecturer provided .NET Microservices Kafka example project
- SRS template used as a guide for document structure: https://www.slideshare.net/slideshow/srs-template-1/46324903

---

# 2. Overall Description

## 2.1 Product Perspective

This project will be developed as a new web based application.

The system will use a microservices architecture instead of building everything as one large backend application.

The project is planned around the following main services.

### Customer and Booking Service

Handles:

- Customers
- Vehicles
- Bookings
- Vehicle check in

### Job and Maintenance Service

Handles:

- Inspections
- Job cards
- Mechanic assignments
- Repair tasks
- Service progress
- Job completion

### Inventory Service

Handles:

- Spare parts
- Available stock
- Part requests
- Parts issued to jobs
- Low stock alerts

### Billing Service

Handles:

- Service charges
- Spare part charges
- Invoices
- Payments

### Notification Service

Handles:

- Booking confirmations
- Service updates
- Invoice notifications
- Vehicle ready notifications

These services will work together using REST APIs and Kafka events.

Main microservice architecture

!Main microservice architecture

Main microservice architecture

## 2.2 Product Functions

The main functions of the system are described below.

### User Management

The system allows different types of users to log in and access the functions related to their role.

### Customer Management

Customer information can be added, viewed, and updated.

### Vehicle Management

Vehicles can be registered under customers and their details can be maintained.

### Service Booking

Customers or service center staff can create service bookings.

### Vehicle Check-In

When a vehicle arrives, staff can check it into the system.

### Inspection and Job Cards

Mechanics can record inspection results and the work that needs to be completed.

### Repair Tracking

The progress of repairs and maintenance can be updated.

### Spare Parts Management

Inventory staff can manage spare parts, stock quantities, requests, and issued parts.

### Billing

Service charges and spare part charges can be added to an invoice.

### Payments

Payments can be recorded against invoices.

### Notifications

Customers can receive updates about bookings and vehicle-service progress.

### Service History

Previous service records can be viewed for each vehicle.

### Reports

Authorized staff can view reports about jobs, inventory, and billing.

## 2.3 User Classes and Characteristics

### 2.3.1 Customer

Customers are vehicle owners who use the system to manage their vehicles and service bookings.

They can:

- Register and log in
- Manage their profile
- Add vehicles
- Create service bookings
- View booking details
- View service progress
- View invoices
- View vehicle service history
- Receive notifications

### 2.3.2 Service Advisor / Receptionist

The Service Advisor manages customer facing activities at the service center.

They can:

- Register customers
- Add vehicles
- Create bookings
- Manage walk in customers
- Check vehicles in
- View active jobs
- Assign mechanics
- View service progress

### 2.3.3 Mechanic

The Mechanic is responsible for inspections and repair work.

They can:

- View assigned jobs
- Record inspection results
- Add repair tasks
- Request spare parts
- Add repair notes
- Update job progress
- Complete assigned tasks

### 2.3.4 Inventory Officer

The Inventory Officer manages spare parts.

They can:

- Add spare parts
- Update part details
- View stock levels
- View part requests
- Issue spare parts
- Monitor low-stock items

### 2.3.5 Accounts / Cashier

The Accounts or Cashier user manages invoices and payments.

They can:

- View completed jobs
- Add charges
- Generate invoices
- Record payments
- View payment information

### 2.3.6 Administrator

The Administrator manages the overall system.

They can:

- Manage staff accounts
- Manage user roles
- View reports
- View dashboard information
- Monitor system activity

## 2.4 Operating Environment

The system will be developed as a web application.

### Frontend

- React.js

### Backend

- ASP.NET Core Web API
- C#

### Database Access

- Entity Framework Core (EF Core)

### Database

- MySQL

### Event Communication

- Apache Kafka
- Confluent.Kafka

### Development Tools

- Docker
- Git
- GitHub
- Swagger
- Postman

### Testing

The following tools will be used for testing:

- xUnit for unit testing
- Selenium for automated user interface and end-to-end testing
- JMeter for performance and load testing
- Swagger and Postman for API testing during development

### CI/CD

- GitHub Actions

Users will access the system through a modern web browser.

### Monitoring and Observability

- Prometheus
- Grafana

Prometheus will be used to collect application and service metrics, while Grafana will be used to display these metrics through dashboards.

Backend and database flow

!Backend and database flow

Backend and database flow

## 2.5 Design and Implementation Constraints

The project will follow these main technical decisions:

- React.js will be used for the frontend.
- ASP.NET Core Web API will be used for backend services.
- C# will be used for backend development.
- Entity Framework Core will be used to interact with the database.
- MySQL will be used for storing system data.
- The backend will be divided into microservices.
- Apache Kafka will be used for selected communication between services.
- REST APIs will be used for communication between the frontend and backend.
- GitHub will be used for version control.
- Automated testing will be included.
- Docker will be used where required for development and running services.
- GitHub Actions will be used for CI/CD.
- Each backend microservice will be developed as an independently runnable ASP.NET Core Web API service.
- Microservices will communicate using well-defined REST APIs and Apache Kafka events.
- Docker and Docker Compose will be used to containerize and run the services.
- Selenium will be used for automated user interface and end-to-end testing.
- JMeter will be used for performance and load testing.
- xUnit will be used for backend unit testing.
- Prometheus and Grafana will be used for monitoring and observability.

## 2.6 Assumptions and Dependencies

The project is based on the following assumptions:

- A customer can have more than one vehicle.
- A vehicle can have many service records.
- Staff will update information when work is completed.
- Inventory staff will record parts that are issued.
- Customers will have access to the internet when using the system.
- The project can be demonstrated using test data.

The system depends on:

- MySQL
- Entity Framework Core
- Apache Kafka
- ASP.NET Core services
- React frontend
- Internet or local network connection

---

# 3. External Interface Requirements

## 3.1 User Interfaces

The system will provide different interfaces depending on the type of user.

### Customer Interface

The customer side may include:

- Login and Registration
- Dashboard
- My Profile
- My Vehicles
- Book a Service
- My Bookings
- Current Service
- Service History
- Invoices
- Notifications

### Service Advisor Interface

The Service Advisor side may include:

- Customer Management
- Vehicle Management
- Booking Management
- Vehicle Check-In
- Active Jobs
- Mechanic Assignment

### Mechanic Interface

The Mechanic side may include:

- Assigned Jobs
- Vehicle Details
- Inspection Form
- Repair Tasks
- Spare-Part Requests
- Repair Notes
- Job Status

### Inventory Interface

The Inventory side may include:

- Spare Parts
- Stock Levels
- Part Requests
- Issue Parts
- Low-Stock Items

### Accounts Interface

The Accounts side may include:

- Invoices
- Service Charges
- Payments
- Completed Jobs

### Administrator Interface

The Administrator side may include:

- User Management
- Role Management
- Dashboard
- Reports

## 3.2 Hardware Interfaces

The first version of the system does not require any special hardware.

The application can be used on:

- Desktop computers
- Laptops
- Tablets
- Smartphones

Devices such as vehicle diagnostic tools, barcode scanners, and IoT sensors are not included in the first version.

## 3.3 Software Interfaces

### React and ASP.NET Core

The React frontend will communicate with the ASP.NET Core Web API through REST APIs.

Example API operations may include:

```
POST /api/bookings
GET /api/bookings/{id}

POST /api/vehicles
GET /api/vehicles/{id}

POST /api/jobs
PUT /api/jobs/{id}/status

GET /api/parts

POST /api/invoices
```

The exact API endpoints will be decided during development.

### ASP.NET Core and MySQL

ASP.NET Core services will use Entity Framework Core to read and write data in MySQL.

Backend and database flow

!Backend and database flow

Backend and database flow

### Microservices and Kafka

Apache Kafka will be used for selected communication between microservices.

## 3.4 Communication Interfaces

The system will mainly use two methods of communication.

### REST APIs

REST APIs will be used when the frontend needs to communicate directly with the backend and expects an immediate response.

### Kafka

Kafka will be used when an activity in one service needs to inform one or more other services that something has happened.

For example, when a service job is completed, both the Billing Service and Notification Service can react to the same event.

Service completion Kafka flow

!Service completion Kafka flow

Service completion Kafka flow

---

# 4. System Features

## 4.1 User Management

### Description

This feature allows users to log in and use the functions available for their role.

**Priority:** High

### Functional Requirements

**FR-UM-01:** The system will allow registered users to log in.

**FR-UM-02:** Invalid login details will be rejected.

**FR-UM-03:** Users will be given access according to their role.

**FR-UM-04:** Users will be able to log out.

**FR-UM-05:** Administrators will be able to create staff accounts.

**FR-UM-06:** Administrators will be able to assign roles.

**FR-UM-07:** Administrators will be able to deactivate staff accounts.

## 4.2 Customer and Vehicle Management

### Description

This feature manages customers and the vehicles registered under them.

**Priority:** High

### Functional Requirements

**FR-CV-01:** Customers will be able to create a profile.

**FR-CV-02:** Staff will be able to register customers.

**FR-CV-03:** Customers will be able to update their basic profile information.

**FR-CV-04:** Customers will be able to register one or more vehicles.

**FR-CV-05:** Staff will be able to register a vehicle for a customer.

**FR-CV-06:** Each vehicle will belong to a customer.

**FR-CV-07:** The system will store the vehicle registration number.

**FR-CV-08:** The system will store details such as make, model, year, and fuel type.

**FR-CV-09:** Customers will be able to view their registered vehicles.

**FR-CV-10:** Authorized staff will be able to update vehicle information.

## 4.3 Service Booking

### Description

Customers and staff can use this feature to create service bookings.

**Priority:** High

### Basic Flow

Service booking flow

!Service booking flow

Service booking flow

### Functional Requirements

**FR-BKG-01:** Customers will be able to create a booking for one of their vehicles.

**FR-BKG-02:** Staff will be able to create a booking for a customer.

**FR-BKG-03:** The booking will include a preferred date.

**FR-BKG-04:** The customer will be able to enter the required service or vehicle problem.

**FR-BKG-05:** The system will create a unique booking ID.

**FR-BKG-06:** Users will be able to view booking details.

**FR-BKG-07:** A booking can be updated before vehicle check-in.

**FR-BKG-08:** A booking can be cancelled before service begins.

**FR-BKG-09:** The system will keep track of the booking status.

**FR-BKG-10:** A `BookingCreated` event can be sent through Kafka after a booking is created.

## 4.4 Vehicle Check-In

### Description

This feature is used when the vehicle arrives at the service center.

**Priority:** High

### Basic Flow

Vehicle check-in flow

!Vehicle check-in flow

Vehicle check-in flow

### Functional Requirements

**FR-CHK-01:** Staff will be able to check in a vehicle with an existing booking.

**FR-CHK-02:** Staff will be able to create a check-in for a walk in customer.

**FR-CHK-03:** The system will record the check-in date and time.

**FR-CHK-04:** Staff will be able to record the current vehicle mileage.

**FR-CHK-05:** Staff will be able to record the customer’s reported problems.

**FR-CHK-06:** The system will update the service status after check-in.

**FR-CHK-07:** A `VehicleCheckedIn` event can be sent through Kafka.

## 4.5 Inspection and Job Management

### Description

This feature is used to manage inspections, repairs, and job progress.

**Priority:** High

### Basic Flow

Inspection and job management flow

!Inspection and job management flow

Inspection and job management flow

### Functional Requirements

**FR-JOB-01:** Staff will be able to create a job card.

**FR-JOB-02:** Each job will have a unique job ID.

**FR-JOB-03:** A job will be connected to a customer and vehicle.

**FR-JOB-04:** A mechanic can be assigned to a job.

**FR-JOB-05:** Mechanics will be able to view their assigned jobs.

**FR-JOB-06:** Mechanics will be able to record inspection results.

**FR-JOB-07:** Mechanics will be able to record problems found during inspection.

**FR-JOB-08:** Repair or service tasks can be added to the job.

**FR-JOB-09:** Mechanics will be able to add repair notes.

**FR-JOB-10:** The system will track the status of the job.

Possible statuses include:

- Awaiting Inspection
- Inspected
- Awaiting Parts
- In Progress
- Completed
- Ready for Collection
- Closed

**FR-JOB-11:** Mechanics or authorized staff will be able to update the job status.

**FR-JOB-12:** The system will record when major job status changes happen.

**FR-JOB-13:** The system will keep track of completed repair tasks.

**FR-JOB-14:** A `ServiceCompleted` event can be sent through Kafka when the job is completed.

## 4.6 Spare Parts Inventory Management

### Description

This feature is used to manage spare parts and track parts used for service jobs.

**Priority:** High

### Basic Flow

Spare parts inventory flow

!Spare parts inventory flow

Spare parts inventory flow

### Functional Requirements

**FR-INV-01:** Inventory staff will be able to add spare parts.

**FR-INV-02:** Each spare part will have a unique ID.

**FR-INV-03:** The system will store the part name and description.

**FR-INV-04:** The system will store the available quantity.

**FR-INV-05:** The system will store the unit price of a part.

**FR-INV-06:** Inventory staff will be able to update part details.

**FR-INV-07:** Users will be able to search for spare parts.

**FR-INV-08:** Mechanics will be able to request spare parts for a job.

**FR-INV-09:** Inventory staff will be able to view part requests.

**FR-INV-10:** Inventory staff will be able to issue parts.

**FR-INV-11:** Parts cannot be issued if enough stock is not available.

**FR-INV-12:** Stock will be reduced after a part is issued.

**FR-INV-13:** The part used will be connected to the related job.

**FR-INV-14:** The system will identify low stock items.

**FR-INV-15:** A `LowStockDetected` event can be sent when stock becomes low.

## 4.7 Billing and Payment Management

### Description

This feature manages service charges, spare part charges, invoices, and payments.

**Priority:** High

### Basic Flow

Billing and payment flow

!Billing and payment flow

Billing and payment flow

### Functional Requirements

**FR-BIL-01:** Staff will be able to add service and labour charges.

**FR-BIL-02:** Spare parts used for the job will be included in the bill.

**FR-BIL-03:** The system will calculate the total amount.

**FR-BIL-04:** Staff will be able to generate an invoice.

**FR-BIL-05:** Each invoice will have a unique invoice ID.

**FR-BIL-06:** The invoice will show customer and vehicle details.

**FR-BIL-07:** The invoice will show service charges and spare-part charges.

**FR-BIL-08:** The invoice will show the total amount.

**FR-BIL-09:** Staff will be able to record payments.

**FR-BIL-10:** The system will update the payment status.

**FR-BIL-11:** Customers will be able to view their invoices.

**FR-BIL-12:** An `InvoiceGenerated` event can be sent after an invoice is created.

## 4.8 Notification Management

### Description

This feature is used to send important updates to customers.

**Priority:** Medium

### Basic Flow

Notification flow

!Notification flow

Notification flow

### Functional Requirements

**FR-NOT-01:** Customers will receive confirmation after a booking is created.

**FR-NOT-02:** Customers can receive updates when important service statuses change.

**FR-NOT-03:** Customers can be informed when the vehicle is ready for collection.

**FR-NOT-04:** Customers can receive invoice related notifications.

**FR-NOT-05:** Notifications can be created when the Notification Service receives relevant Kafka events.

**FR-NOT-06:** Customers will be able to view available notifications in the system.

The final notification method, such as email or SMS, will be decided later.

## 4.9 Vehicle Service History

### Description

This feature keeps previous vehicle service records.

**Priority:** Medium

### Functional Requirements

**FR-HIS-01:** Completed service jobs will be saved as part of the vehicle service history.

**FR-HIS-02:** Customers will be able to view the service history of their vehicles.

**FR-HIS-03:** Staff will be able to view previous service records.

**FR-HIS-04:** Service history will show the service date.

**FR-HIS-05:** Service history will show repairs and maintenance completed.

**FR-HIS-06:** Service history will show spare parts used.

**FR-HIS-07:** Recorded mileage can be displayed in the history.

**FR-HIS-08:** Previous records will remain available when a new service job is created.

## 4.10 Dashboard and Reporting

### Description

This feature gives staff a quick view of important information.

**Priority:** Medium

### Functional Requirements

**FR-REP-01:** Staff will be able to view active service jobs.

**FR-REP-02:** Jobs can be viewed according to their status.

**FR-REP-03:** Inventory staff will be able to view current stock levels.

**FR-REP-04:** Inventory staff will be able to view low-stock items.

**FR-REP-05:** Staff will be able to view the service history of a selected vehicle.

**FR-REP-06:** Accounts staff will be able to view invoice and payment information.

**FR-REP-07:** Reports will only be available to users with the required permissions.

---

# 5. Non-Functional Requirements

## 5.1 Performance

**NFR-01:** Normal system pages should load within a reasonable time under the expected project workload.

**NFR-02:** The system should respond quickly when users search for customers, vehicles, jobs, or spare parts.

**NFR-03:** Kafka events should be processed without unnecessary delays.

## 5.2 Security

**NFR-04:** Users must log in before accessing protected system functions.

**NFR-05:** Users must only be able to access functions allowed for their role.

**NFR-06:** Passwords must be stored securely.

**NFR-07:** Customers must not be able to view another customer’s private vehicle or service records.

**NFR-08:** User input should be validated before being saved.

**NFR-09:** Sensitive configuration details should not be stored directly in public source code.

## 5.3 Reliability

**NFR-10:** Important information should not be lost if one service temporarily stops working.

**NFR-11:** A problem with the Notification Service should not stop other important operations such as creating a booking.

**NFR-12:** Errors should be logged so that developers can identify the problem.

## 5.4 Usability

**NFR-13:** The user interface should be simple and consistent.

**NFR-14:** Forms should clearly show required fields.

**NFR-15:** Validation errors should be easy to understand.

**NFR-16:** Important statuses should be clearly visible.

## 5.5 Maintainability

**NFR-17:** Each microservice should have a clear responsibility.

**NFR-18:** Code should follow an agreed structure and naming style.

**NFR-19:** Source code will be stored using Git and GitHub.

**NFR-20:** Services should avoid unnecessary dependencies on each other.

## 5.6 Scalability

**NFR-21:** Services should be able to run separately.

**NFR-22:** Individual services should be able to be scaled separately if needed.

## 5.7 Testing

Testing will be carried out at different levels to make sure the main functions of the system work correctly.

### Unit Testing

xUnit will be used to test important backend business logic.

Examples include:

- Stock availability checks
- Invoice calculations
- Booking validation
- Job status changes

### User Interface and End-to-End Testing

Selenium will be used to automate selected user workflows.

Examples include:

- User login
- Vehicle registration
- Creating a service booking
- Viewing a booking
- Checking service status

### Performance Testing

JMeter will be used to test the performance of selected APIs and system operations under multiple requests.

This can be used to check:

- Response time
- Number of requests handled
- Behaviour under increased load

### API Testing

Swagger and Postman may be used during development to test REST API endpoints.

### Requirements

**NFR-23:** Important backend business logic will have unit tests.

**NFR-24:** xUnit will be used for backend unit testing.

**NFR-25:** Selenium will be used for selected user interface and end-to-end tests.

**NFR-26:** JMeter will be used for performance and load testing.

## 5.8 Monitoring and Observability

**NFR-27:** The system will provide basic monitoring for the main backend microservices.

**NFR-28:** Prometheus will be used to collect available application and service metrics.

**NFR-29:** Grafana will be used to display monitoring information through dashboards.

**NFR-30:** The team will be able to identify whether the main microservices are running during testing and demonstration.

**NFR-31:** Important application errors will be logged so that they can be investigated.

---

# 6. Other Requirements

## 6.1 Database Requirements

MySQL will be used to store the system data.

Entity Framework Core will be used in the ASP.NET Core backend to communicate with MySQL.

Backend and database flow

!Backend and database flow

Backend and database flow

The main data areas include:

### Customer and Booking Data

- Customers
- Vehicles
- Bookings
- Check-Ins

### Job Data

- Job Cards
- Inspections
- Repair Tasks
- Mechanic Assignments
- Repair Notes
- Job Status

### Inventory Data

- Spare Parts
- Stock
- Part Requests
- Part Issues

### Billing Data

- Invoices
- Invoice Items
- Payments

### Notification Data

- Notifications

Each microservice will mainly manage the data related to its own area.

## 6.2 Microservice Requirements

The system will initially contain the following main services:

1. Customer and Booking Service
2. Job and Maintenance Service
3. Inventory Service
4. Billing Service
5. Notification Service

### Main Architecture

Main microservice architecture

!Main microservice architecture

Main microservice architecture

The React frontend communicates with the backend services using REST APIs. Each backend service is developed using ASP.NET Core Web API and C#. Entity Framework Core is used to communicate with MySQL.

Apache Kafka is used for selected asynchronous communication between the microservices.

## 6.3 Kafka Communication

Kafka will be used for important events between services.

### Main Events

| Event | Produced By | Used By |
| --- | --- | --- |
| BookingCreated | Customer and Booking Service | Notification Service |
| VehicleCheckedIn | Customer and Booking Service | Job and Maintenance Service |
| PartRequested | Job and Maintenance Service | Inventory Service |
| PartIssued | Inventory Service | Job and Maintenance Service, Billing Service |
| LowStockDetected | Inventory Service | Notification Service / Admin |
| ServiceCompleted | Job and Maintenance Service | Billing Service, Notification Service |
| InvoiceGenerated | Billing Service | Notification Service |
| PaymentRecorded | Billing Service | Notification Service |

### Service Completion Example

Service completion Kafka flow

!Service completion Kafka flow

Service completion Kafka flow

### Spare-Part Issue Example

Part issue Kafka flow

!Part issue Kafka flow

Part issue Kafka flow

Kafka is mainly used when multiple services need to react after something important happens in the system. REST APIs will still be used where a direct response is needed.

## 6.4 CI/CD

The project source code will be stored on GitHub.

GitHub Actions can be used to automatically:

1. Get the latest code.
2. Restore dependencies.
3. Build the application.
4. Run automated tests.
5. Report build or test errors.
6. Prepare the application for deployment.

The final hosting platform will be decided during development.

## 6.5 Deployment and Monitoring

The microservices will be designed so that they can run independently.

Docker will be used to containerize the application services and supporting components.

Docker Compose may be used during development and demonstration to run the required services together.

The planned environment may include:

- React frontend
- Customer and Booking Service
- Job and Maintenance Service
- Inventory Service
- Billing Service
- Notification Service
- MySQL
- Apache Kafka
- Prometheus
- Grafana

A simplified deployment structure is:

React Frontend

↓

ASP.NET Core Microservices

↓

MySQL / Apache Kafka

Monitoring:

ASP.NET Core Microservices

↓

Prometheus

↓

Grafana Dashboard

The final hosting environment may be decided later during the implementation stage.

---

# Appendix A - Glossary

| Term | Meaning |
| --- | --- |
| API | A way for different software parts to communicate |
| ASP.NET Core | The backend framework used for the project |
| C# | The programming language used to develop the ASP.NET Core backend |
| CI/CD | An automated process used to build, test, and deploy software |
| CRUD | Create, Read, Update, and Delete |
| EF Core | Entity Framework Core, used by the backend to work with the database |
| Event | Information showing that something important has happened in the system |
| Kafka | The platform used to send events between microservices |
| Microservice | A smaller backend service responsible for a particular part of the system |
| MySQL | The database used by the project |
| React | The technology used to build the frontend |
| REST API | An API used for direct communication between the frontend and backend |
| SRS | Software Requirements Specification |

---

# Appendix B - System Models

## B.1 Main Actors

The main actors of the system are:

- Customer
- Service Advisor / Receptionist
- Mechanic
- Inventory Officer
- Accounts / Cashier
- Administrator

## B.2 Main System Workflow

Full vehicle service workflow

!Full vehicle service workflow

Full vehicle service workflow

The workflow starts when the customer creates a service booking and continues through vehicle check in, inspection, repairs, parts usage, billing, payment, vehicle collection, and service history.

## B.3 Main Microservice Architecture

Main microservice architecture

!Main microservice architecture

Main microservice architecture

## B.4 Example Kafka Flow

Service completion Kafka flow

!Service completion Kafka flow

Service completion Kafka flow

---

# Appendix C - Future Improvements

The following features can be considered in future versions of the system:

- Online payments
- SMS notifications
- Supplier management
- Automatic spare part reordering
- Mobile application
- Automated service reminders
- Multi branch service center support
- Advanced reports
- Predictive maintenance
- Vehicle diagnostic device integration
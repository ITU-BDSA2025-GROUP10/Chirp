---
title: _Chirp!_ Project Report (Techical documentation)

---

# _Chirp!_ Project Report

**ITU BDSA 2025 – Group 10**

**Authors**  
Andreas Bank Hyldal `<ahyl@itu.dk>`  
Cornelius Baasch Andersen `<coan@itu.dk>`  
Jacob Folkmann Præstegaard `<jafo@itu.dk>`  
Jacob Hørberg `<jacho@itu.dk>`  
Jogvan Andreas á Lad Jacobsen `<jogv@itu.dk>`

---

# Table of Contents
<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->


- [1. Design and Architecture](#1-design-and-architecture)
  - [1.1 Domain Model](#11-domain-model)
  - [1.2 Architecture — In the Small](#12-architecture--in-the-small)
  - [1.3 Architecture of the Deployed Application](#13-architecture-of-the-deployed-application)
  - [1.4 User Activities](#14-user-activities)
      - [1.4.1 Activity diagram for unauthorized user](#141-activity-diagram-for-unauthorized-user)
      - [1.4.2 Activity diagram for authenticated users](#142-activity-diagram-for-authenticated-users)
  - [1.5 Sequence of Functionality / Calls Through Chirp!](#15-sequence-of-functionality--calls-through-chirp)
      - [1.5.1 UML Sequence Diagram](#151-uml-sequence-diagram)
- [2. Process](#2-process)
  - [2.1 Build, Test, Release, and Deployment](#21-build-test-release-and-deployment)
      - [2.1.1 GitHub Actions — Activity Diagram](#211-github-actions--activity-diagram)
      - [2.1.2 Summary of CI/CD Execution](#212-summary-of-cicd-execution)
  - [2.2 Team Work](#22-team-work)
      - [2.2.1 Project Board Snapshot (Before Submission)](#221-project-board-snapshot-before-submission)
  - [2.3 How to Make Chirp! Work Locally](#23-how-to-make-chirp-work-locally)
  - [2.4 How to Run the Test Suite Locally](#24-how-to-run-the-test-suite-locally)
      - [2.4.1 Execution Steps](#241-execution-steps)
      - [2.4.2 Types of Tests & Purpose](#242-types-of-tests--purpose)
- [3. Ethics](#3-ethics)
  - [3.1 License](#31-license)
  - [3.2 LLMs, ChatGPT, Copilot & AI Tools Used](#32-llms-chatgpt-copilot--ai-tools-used)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

---

# 1. Design and Architecture

## 1.1 Domain Model

![client-server-diagram](https://hackmd.io/_uploads/r1cNaOpQZe.png)


The domain model includes the entities Author, Cheep, Comment, and Followig. Authors represent users of the system and can create multiple Cheeps, each of which belongs to exactly one Author.

Authors can write Comments on Cheeps. Each Comment is associated with one Author and one Cheep, while a Cheep can have multiple Comments.

Social relationships between Authors are handled through the Following entity, which models the many-to-many relationship of users following each other. The structure supports posting, commenting, and user connections within the system.


---

## 1.2 Architecture — In the Small
![Composite Structure Diagram1](https://hackmd.io/_uploads/rJbEnua7Wg.png)

The diagram illustrates representative dependencies between layers following the onion architecture principle. All presentation layer components (page models) depend on infrastructure services and repositories in a similar manner to the PublicModel example shown. Similarly, all repositories depend on their corresponding domain entities and DTOs following the pattern demonstrated by CheepRepository. The dependency direction is strictly enforced: outer layers depend on inner layers, while the Core layer has zero dependencies on outer layers, maintaining the integrity of the onion architecture.



## 1.3 Architecture of the Deployed Application

![Untitled](https://hackmd.io/_uploads/HJLnhdp7Ze.jpg)


The Chirp application is deployed as a client-server web application on Microsoft Azure App Service. The server consists of three layers: Chirp.Web, which handles the user interface and HTTP requests, Chirp.Core, which contains the domain models and business logic, and Chirp.Infrastructure, which manages data persistens using repositories and Entitiy Framework Core.

Users access the application through a web browser over HTTPS. Application data is stored in a SQLite database accessed by the infrastructure layer. Authentication is handled via GitHub OAuth, where Chirp.Web communicates with GitHub's Authentication API when users log in.



---

## 1.4 User Activities
Unauthorized users start their user journey on the public timeline where they have the possibility to register or login to become an authorized user. Unauthorized users can view all cheeps and comments on the public timeline and they can switch between pages by clicking next/previous. They can also view a specific user's timeline by clicking the authors username.
Once a user has been authorized, either by filling out the register form and confirming their email or by logging in and authorizing through GitHub, they have the same possibilities as an unauthorized user. In addition, they are now able to post and comment cheeps. They can follow and unfollow other users and access a "following timeline", containing cheeps from the followed users. They can also access "my timeline" containing their own cheeps. Authorized users also have an "about me" page, where they can view the personal information that is stored about them such as username, email, the followed users and the cheeps they have posted. In the about me page, the user also have the option to delete their account and all the stored information about themselves, using the "forget me!" upon confirming with their password.

Below are the two activity diagrams for both unauthorized and authorized users. The internal pages are illustrated as orange boxes, actions as green boxes and external pages as blue boxes. To improve readability of the diagrams, we omit arrows representing navigation back to previously visited pages. In the application, users can freely navigate between all pages at any time by clicking the page they want to use.
#### 1.4.1 Activity diagram for unauthorized user
![UnauthorizedActivityDiagram](https://hackmd.io/_uploads/Syu-YQl4Wx.jpg)

#### 1.4.2 Activity diagram for authenticated users
![Authorized Activity Diagram](https://hackmd.io/_uploads/rk2jfXxNZe.jpg)


---

## 1.5 Sequence of Functionality / Calls Through Chirp!
#### 1.5.1 UML Sequence Diagram

![sequenceOfCalls](https://hackmd.io/_uploads/SkgRIZgVbl.png)

The sequence diagram shows the flow of calls when an unauthorized user requests the public timeline in the Chirp! application by sending an HTTP GET request to the root endpoint.

The request is recieved by the Chirp.Web layer, which routes it to the `PublicModel.OnGet(pageIndex method)` of the Razor Page. Since the user is not authenticated, the request is handled as a public request.

The page model calls `CheepService.GetCheeps(currentUser = null)`in the Chirp.Infrastructure layer to retrieve the relevant cheeps. The service queries the database using Entity Framework Core, which returns the public cheep data. This data is processed and returned to the web layer as a list of `CheepViewModel` objects.

Finally the `Public.cshtml` Razor view is rendered using the retrieved data, and the resulting HTML page is returned to the client.

The diagram illustrates the complete flow from the initial HTTP request through internal C# method calls and database access to the fully rendered web page.


---

# 2. Process

## 2.1 Build, Test, Release, and Deployment
#### 2.1.1 GitHub Actions — Activity Diagram

![client-server-diagram](https://hackmd.io/_uploads/ry5kD6yNbg.png)

The diagram illustrates our GitHub Actions workflow used to build, test, release and deploy the Chirp! application.
Development starts by createing a feature branch, commiting changes and opening a pull request. When the pull request is created a CI workflow is triggered that restores dependencies, builds the application and runs automated tests, while the pull request is reviewed in parallel. If the CI checks succeed and the pull request is approved, the changes are merged into the main branch, which triggers the build and deployment workflow that deploys the application to Azure Web App. If a release is required, a version tag is pushed, triggering a release workflow that builds, tests and publishes platform-specific binaries.
Finally developers pull the updated main branch.

#### 2.1.2 Summary of CI/CD Execution
---

## 2.2 Team Work
#### 2.2.1 Project Board Snapshot (Before Submission)

---

## 2.3 How to Make Chirp! Work Locally

---

## 2.4 How to Run the Test Suite Locally
#### 2.4.1 Execution Steps
#### 2.4.2 Types of Tests & Purpose

---

# 3. Ethics

## 3.1 License

---

## 3.2 LLMs, ChatGPT, Copilot & AI Tools Used
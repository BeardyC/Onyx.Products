# Onyx Products API

[![Build, Test](https://github.com/BeardyC/Onyx.Products/actions/workflows/dotnet.yml/badge.svg?branch=feature%2Fpipelines)](https://github.com/BeardyC/Onyx.Products/actions/workflows/dotnet.yml)

---
![Architecture Diagram](./docs/system-diagram.png)

## Overview
This is a minimal, production-grade .NET 8 Web API for managing Products, designed as part of the Onyx Commodities coding test.

- [Running](#running)
- [Products API Service](#products-api-service)
  - [Authentication](#authentication)
  - [Health Check](#health-check)
  - [Endpoints](#endpoints)
    - [Authentication](#authentication-1)
    - [Products](#products)
- [Swagger Documentation](#swagger-documentation)

## Running

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)/[Podman](https://podman.io/)
- [DOTNET SDK 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Rider](https://www.jetbrains.com/rider/)/[Visual Studio 2022](https://visualstudio.microsoft.com/vs/)

Start SQL server 
```ps
podman compose up -d -f ./docker-compose.yml
```
You'll then have to manually run the scripts in your IDE of choice - [DBUp](https://dbup.readthedocs.io/en/latest/) or similar would be used to deploy the versioned sql scripts but it's out of scope for this.

```ps
dotnet run --launch-profile https --configuration Release --project .\src\Onyx.ProductManagement.Api\
```

Nagivate to `https://localhost:7154/swagger/index.html` to test out the API.


### Products API Service

The Products API hosts a RESTful service for managing product data and basic user authentication. It also exposes a [Swagger UI](https://swagger.io/) endpoint for easy exploration and testing in local and development environments.

#### Authentication

Most API endpoints require a valid **JWT Bearer Token** for authentication, **except** for `/health` endpoint.  
Generate one using the `/auth/{user}` endpoint.
Include the token in the `Authorization` header when making requests:

```
Authorization: Bearer {your-jwt-token}
```

Example:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...
```

## Health Check

- `GET /health`  
  Simple endpoint to verify the service is running.
   _No authentication required._

  **Response codes:**
  - `200 OK` - Service is healthy and reachable.

---

## Endpoints

### Authentication

- `GET /auth/{user}`  
  Retrieve authentication information for a specific user.  
  _No authentication required._

  **Path parameters:**
  - `user` (string): The username.

  **Response codes:**
  - `200 OK` - User authentication information retrieved successfully.

---

### Products

- `POST /v1/products`  
  Create a new product.  
  _Requires Bearer Token authentication._

  **Request body example:**

  ```json
  {
      "name": "T-Shirt",
      "price": 19.99,
      "colour": "Red"
  }
  ```

  **Response codes:**
  - `200 OK` - Product created successfully.

---

- `GET /v1/products`  
  Retrieve a list of all products, with optional paging.  
  _Requires Bearer Token authentication._

  **Query parameters:**
  - `PageNumber` (integer, optional): Page number to retrieve.
  - `PageSize` (integer, optional): Number of items per page.

  **Response example:**

  ```json
  [
      {
          "id": 1,
          "name": "T-Shirt",
          "price": 19.99,
          "colour": "Red"
      },
      {
          "id": 2,
          "name": "Hat",
          "price": 9.99,
          "colour": "Blue"
      }
  ]
  ```

  **Response codes:**
  - `200 OK` - Products retrieved successfully.

---

- `GET /v1/products/colour/{colour}`  
  Retrieve products filtered by colour.  
  _Requires Bearer Token authentication._

  **Path parameters:**
  - `colour` (string): The colour to filter by.

  **Example request:**

  ```
  GET /v1/products/colour/Red
  ```

  **Response example:**

  ```json
  [
      {
          "id": 1,
          "name": "T-Shirt",
          "price": 19.99,
          "colour": "Red"
      }
  ]
  ```

  **Response codes:**
  - `200 OK` - Products with the specified colour retrieved successfully.

---

# Swagger Documentation

You can access the full interactive API documentation via Swagger UI when running locally or in your development environment.

---

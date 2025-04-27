# Onyx Products API

[![Build, Test](https://github.com/BeardyC/Onyx.Products/actions/workflows/dotnet.yml/badge.svg?branch=feature%2Fpipelines)](https://github.com/BeardyC/Onyx.Products/actions/workflows/dotnet.yml)

---
![Architecture Diagram](./docs/system-diagram.png)

## Overview
This is a minimal, production-grade .NET 8 Web API for managing Products, designed as part of the Onyx Commodities coding test.

- [Running](#running)
- [Products API Service](#products-api-service)
  - [Authentication](#authentication)
  - [Endpoints](#endpoints)
    - [Health Check](#health-check)
    - [Authentication](#authentication-1)
    - [Products](#products)
- [Swagger Documentation](#swagger-documentation)

## Running

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)/[Podman](https://podman.io/)
- [DOTNET SDK 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Rider](https://www.jetbrains.com/rider/)/[Visual Studio 2022](https://visualstudio.microsoft.com/vs/)

Start SQL server 
```ps
podman compose -f ./docker-compose.yml up -d
```
You'll then have to manually run the scripts in your IDE of choice - [DBUp](https://dbup.readthedocs.io/en/latest/) or similar would be used to deploy the versioned sql scripts but it's out of scope for this.

For the API service, you can either build and run the docker file or you can simply run the below
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


## Endpoints

## Health Check

- `GET /health`  
  Simple endpoint to verify the service is running.
   _No authentication required._

  **Response codes:**
  - `200 OK` - Service is healthy and reachable.
  - `503 Service Unavailable` - Service is unhealthy or degraded.
 
  **Response example:**

  ```json
  {
    "status": "Healthy",
    "checks": [
      {
        "name": "api_status",
        "status": "Healthy",
        "description": "API is running.",
        "duration": 0.0000031
      },
      {
        "name": "products_db",
        "status": "Healthy",
        "description": null,
        "duration": 0.0011731
      }
    ]
  }
  ```
---

### Authentication

- `GET /auth/{user}`  
  Retrieve authentication information for a specific user.  
  _No authentication required._

  **Path parameters:**
  - `user` (string): The username.

  **Response codes:**
  - `200 OK` - User authentication information retrieved successfully.
  - `400 Bad Request` - Returned if the username is not provided.
  - `401 Unauthorized` - Returned if the specified user is unknown or invalid.


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
  - `201 Created` - Product created successfully. The response includes the ID of the created product and a Location header pointing to the new resource.
  - `400 Bad Request` - Returned for invalid product data due to validation errors.
  - `401 Unauthorized` - Returned if a valid JWT Bearer Token is not provided.
  - `409 Conflict` - Returned if an already existing product exists with that given name (Unique Constraint on Name column).
  - `500 Internal Server Error` - Returned if an unexpected error occurs during product creation.

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
  - `401 Unauthorized` - Returned if a valid JWT Bearer Token is not provided.
  - `500 Internal Server Error` - Returned if an unexpected error occurs while fetching products. 

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
  - `401 Unauthorized` - Returned if a valid JWT Bearer Token is not provided.
  - `500 Internal Server Error` - Returned if an unexpected error occurs while fetching products by colour.    

Sources and related content


---

# Swagger Documentation

You can access the full interactive API documentation via Swagger UI when running locally or in your development environment.

---

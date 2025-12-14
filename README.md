# FreeMarket Tech Challenge - Shopping Basket API

A comprehensive shopping basket API built with .NET 9.0, implementing CQRS pattern with MediatR, Entity Framework Core, and clean architecture principles.

## Technical Improvements
### Improvement I would do if I have more time
- Database migrations and indexing for frequently accessed data
- Proper resource disposal and connection management
- Request validation
- Implement authentication and authorization
- Caching strategies for discount codes and product data
- Rate limiting, throttling and appropriate CORS policies
- Configure proper logging and monitoring
- API versioning strategy

## 🏗️ Solution Overview

**Architecture**: Clean architecture with CQRS pattern using MediatR
**Database**: Entity Framework Core with In-Memory database for development
**API Documentation**: Swagger/OpenAPI integration
**Testing**: Comprehensive unit tests with xUnit

### Key Technologies
- **.NET 9.0** - Latest .NET framework
- **ASP.NET Core Web API** - RESTful API framework
- **MediatR** - CQRS and Mediator pattern implementation
- **Entity Framework Core** - ORM with In-Memory database
- **Swagger/OpenAPI** - Interactive API documentation

### Project Structure
```
FreeMarket.Tech.Challenge/
├── FreeMarket.Tech.Challenge.Api/          # Main Web API project
│   ├── Context/                           # Database context
│   ├── DTOs/                              # Data transfer objects
│   ├── Entities/                          # Domain entities
│   ├── Features/Basket/                   # CQRS basket operations
│   ├── Services/                          # Business services
│   └── Extensions/                        # Extension methods
├── FreeMarket.Tech.Challenge.Api.Tests/    # Unit tests
└── FreeMarket.Tech.Challenge.sln          # Solution file
```

## 🎯 Key Features

### Shopping Basket Operations
- **Basket Management**: Create, retrieve, and manage shopping baskets
- **Item Management**: Add/remove products and update quantities
- **Discount System**: Apply percentage-based discount codes
- **Cost Calculation**: Automatic VAT calculation (20%) and pricing
- **Shipping**: Address management and shipping cost calculation

### Available Discount Codes (Seed Data)
- `SAVE10` - 10% discount (✅ Valid)
- `EXPIRED20` - 20% discount (❌ Expired - for testing error scenarios)

## 🚀 Quick Start

### Prerequisites
- **.NET 9.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/9.0)
- **IDE**: Visual Studio 2022, JetBrains Rider, or VS Code

### Setup & Run

1. **Clone and setup**
   ```bash
   git clone <repository-url>
   cd FreeMarket.Tech.Challenge
   dotnet restore
   dotnet build
   ```

2. **Run the API**
   ```bash
   # Option 1: Using .NET CLI
   cd FreeMarket.Tech.Challenge.Api
   dotnet run

   # Option 2: Using Visual Studio/Rider
   # Open FreeMarket.Tech.Challenge.sln and press F5
   ```

3. **Access the application**
   - **API Base URL**: `http://localhost:5209`
   - **Swagger UI**: `http://localhost:5209/swagger`
   - **HTTPS**: `https://localhost:7181`

### 🐳 Docker Option (Easiest Testing)

For the quickest setup without installing .NET SDK:

```bash
# Build and run with Docker
docker build -t freemarket-basket-api .
docker run -p 8080:80 -e ASPNETCORE_ENVIRONMENT=Development freemarket-basket-api

# Or use docker-compose (if available)
docker-compose up --build
```

**Docker Endpoints**:
- **API Base URL**: `http://localhost:8080`
- **Swagger UI**: `http://localhost:8080/swagger`

### 📊 Seed Data for Testing

The application automatically populates the in-memory database with sample data on startup, perfect for testing:

#### Available Test Basket
- **Basket ID**: `D2F1C5E4-8F4B-4C3A-9D6A-1E2B3C4D5E6F`
- **Customer**: john.doe@example.com
- **Contains**: 3 items with mixed discounted and non-discounted products

#### Sample Products Available
| Product ID | Name | Price | Discounted | Stock |
|------------|------|-------|------------|-------|
| `3BDA8B6A-A189-4D10-9B74-ED0BE50694D9` | Laptop Pro X1 | $1,299.99 | No | 8 |
| `FFC53C01-97FA-40A4-B1F4-54FE1BC3B0FA` | Wireless Ergonomic Mouse | $34.99 | Yes ($29.99) | 45 |
| `6bf47d60-d900-41ce-95d8-968a3b34a968` | Mechanical Gaming Keyboard | $89.99 | Yes ($69.99) | 12 |
| `0343e51b-f134-49f5-97cc-d0cb4535644b` | USB-C Hub Premium | $59.99 | No | 0 (Out of stock) |
| `E8A7B5C3-9D2F-4E6A-8B1C-3F5E7D9A2B4C` | 4K Webcam Pro | $149.99 | No | 18 |
| `C4F8E2D1-7A9B-3C5E-9F2D-6B8A1C4E7F9A` | Monitor Light Bar | $79.99 | No | 35 |

#### Discount Codes for Testing
- **`SAVE10`** - 10% discount (✅ Valid)
- **`EXPIRED20`** - 20% discount (❌ Expired)

#### Quick Test Example
```bash
# Local .NET runtime
curl -X GET "http://localhost:5209/api/baskets/D2F1C5E4-8F4B-4C3A-9D6A-1E2B3C4D5E6F" \
  -H "Accept: application/json"

# Docker container
curl -X GET "http://localhost:8080/api/baskets/D2F1C5E4-8F4B-4C3A-9D6A-1E2B3C4D5E6F" \
  -H "Accept: application/json"

# Apply discount code (local)
curl -X POST "http://localhost:5209/api/baskets/D2F1C5E4-8F4B-4C3A-9D6A-1E2B3C4D5E6F/apply-discount" \
  -H "Content-Type: application/json" \
  -d '{"discountCode": "SAVE10"}'
```

## 🧪 Testing

### Run All Tests
```bash
dotnet test
```

### Run Tests for Specific Project
```bash
dotnet test FreeMarket.Tech.Challenge.Api.Tests/
```

### Run Tests with Verbosity
```bash
dotnet test --verbosity normal
```

### Test Coverage
The test suite includes comprehensive unit tests covering:
- Basket operations (CRUD)
- Discount code validation and application
- Price calculations
- Error handling and edge cases
- Business logic validation

## 📚 API Endpoints

### Basket Operations
- `POST /api/baskets` - Create a new basket
- `GET /api/baskets/{id}` - Get basket by ID
- `POST /api/baskets/{id}/items` - Add item to basket
- `PUT /api/baskets/{basketId}/items/{itemId}/quantity/{quantity}` - Update item quantity
- `DELETE /api/baskets/{id}/items/{itemId}` - Remove item from basket
- `POST /api/baskets/{id}/discount` - Apply discount code
- `POST /api/baskets/{id}/shipping` - Set shipping address
- `GET /api/baskets/{id}/total` - Get total with VAT
- `GET /api/baskets/{id}/total/excluding-vat` - Get total without VAT

### Sample Request Formats

#### Create Basket
```json
POST /api/baskets
{
  "customerEmail": "customer@example.com"
}
```

#### Add Item to Basket
```json
POST /api/baskets/{basketId}/items
{
  "productId": "guid",
  "quantity": 2
}
```

#### Update Item Quantity
```http
PUT /api/baskets/{basketId}/items/{itemId}/quantity/5
```

#### Apply Discount Code
```json
POST /api/baskets/{basketId}/discount
{
  "code": "SAVE10"
}
```

## 🔧 Configuration

### Database Configuration
- Uses **In-Memory Database** for development and testing
- Database is automatically seeded with sample data on startup
- Entity Framework migrations are handled via code-first approach

## 🧩 Project Structure Details

### Core Components

#### Entities (`/Entities/`)
- `Basket` - Main basket entity with business logic
- `BasketItem` - Individual items in the basket
- `Product` - Product information
- `Discount` - Discount code management
- `Address` - Shipping address details
- `ShippingRate` - Shipping cost calculations

#### DTOs (`/DTOs/`)
- `BasketDTO` - Basket data transfer object
- `BasketItemDto` - Basket item DTO
- `DiscountDto` - Discount information DTO
- `AddressDto` - Address information DTO

#### Features (`/Features/Basket/`)
- CQRS pattern implementation for each basket operation
- Separate commands and queries for different operations
- Clean separation of concerns

#### Services (`/Services/`)
- `ShippingCostService` - Shipping cost calculation logic

#### Extensions (`/Extensions/`)
- Extension methods for entity-to-DTO mapping

## 🎯 Development Best Practices

### Code Quality
- **Clean Architecture** - Separation of concerns with layered architecture
- **CQRS Pattern** - Command Query Responsibility Segregation
- **Repository Pattern** - Data access abstraction
- **Dependency Injection** - Loose coupling and testability
- **Async/Await** - Non-blocking operations

### Testing Strategy
- **Unit Tests** - Comprehensive coverage of business logic
- **In-Memory Database** - Isolated testing environment
- **AAA Pattern** - Arrange, Act, Assert test structure
- **Edge Cases** - Testing boundary conditions and error scenarios

### Error Handling
- **Global Exception Handling** - Consistent error responses
- **Validation** - Input validation with meaningful error messages
- **HTTP Status Codes** - Appropriate status codes for different scenarios

# Level 2 Task 3 — JWT Authentication & Authorization

Complete auth system with JWT tokens, refresh tokens, role-based access.

## Features
- Register/Login with JWT access tokens
- Refresh token rotation
- Role-based authorization (Admin, Manager, User)
- Policy-based authorization
- Password hashing with PBKDF2
- Swagger UI with Bearer token support

## Test with curl
```bash
# Register
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"gildas","email":"niygildas@gmail.com","password":"Password123!"}'

# Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"niygildas@gmail.com","password":"Password123!"}'

# Use token
curl http://localhost:5000/api/resources/public-data \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## Default users
- admin@codveda.com / Admin123! (Role: Admin)
- manager@codveda.com / Manager123! (Role: Manager)
- niygildas@gmail.com / Gildas123! (Role: User)

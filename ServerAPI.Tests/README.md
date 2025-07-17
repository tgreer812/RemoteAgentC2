# ServerAPI.Tests

This project contains comprehensive unit tests for the RemoteAgentServerAPI controllers.

## Overview

The test project provides unit test coverage for:
- **TaskingController** - Tests for job retrieval and task submission endpoints
- **JobController** - Tests for job management operations

## Test Coverage

### TaskingController Tests
- ✅ Constructor validation (null database parameter)
- ✅ GET endpoint scenarios:
  - No jobs exist
  - Jobs exist but all sent
  - Mixed job statuses (only pending returned)
  - Pending jobs exist (jobs returned and marked as sent)
- ✅ POST endpoint scenarios:
  - Valid agent task submission
  - Invalid model state

### JobController Tests
- ✅ Constructor validation (null database parameter)
- ✅ GET endpoint scenarios:
  - Job exists
  - Job does not exist
- ✅ POST endpoint scenarios:
  - Valid job creation
  - Null job/job data validation
  - Empty job type validation
  - Invalid agent ID
  - Database save failures
  - Database exceptions
- ✅ PUT endpoint scenarios:
  - Valid plugin result update
  - Null plugin result validation
  - Job not found
  - Update failures
  - Database exceptions

## Technologies Used

- **xUnit** - Testing framework
- **Moq** - Mocking framework for creating test doubles
- **FluentAssertions** - Fluent assertion library for readable test assertions
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing utilities for ASP.NET Core

## Test Structure

Each test class follows the Arrange-Act-Assert (AAA) pattern:
- **Arrange**: Set up test data and mock dependencies
- **Act**: Execute the method being tested
- **Assert**: Verify the expected behavior

## Running Tests

To run all tests:
```bash
dotnet test
```

To run tests with detailed output:
```bash
dotnet test --verbosity normal
```

To run tests for a specific controller:
```bash
dotnet test --filter "FullyQualifiedName~TaskingControllerTests"
dotnet test --filter "FullyQualifiedName~JobControllerTests"
```

## Test Results

All 22 tests currently pass:
- 8 tests for TaskingController
- 14 tests for JobController

The tests provide comprehensive coverage of both happy path scenarios and error conditions, ensuring the controllers behave correctly under various circumstances.

# Contributing to Fuddy-Duddy

Thank you for your interest in contributing to Fuddy-Duddy! This document provides guidelines and instructions for contributing to this project.

## Code of Conduct

By participating in this project, you agree to abide by our Code of Conduct:

- Be respectful and inclusive
- Provide constructive feedback
- Focus on what is best for the community
- Show empathy towards other community members

## How to Contribute

### Reporting Bugs

If you find a bug, please create an issue with the following information:

- A clear, descriptive title
- Steps to reproduce the issue
- Expected behavior
- Actual behavior
- Screenshots if applicable
- Environment details (OS, browser, etc.)

### Suggesting Enhancements

We welcome suggestions for enhancements! Please create an issue with:

- A clear, descriptive title
- Detailed description of the proposed enhancement
- Any relevant examples or mockups
- Explanation of why this enhancement would be useful

### Pull Requests

1. Fork the repository
2. Create a new branch from `main`
3. Make your changes
4. Add or update tests as needed
5. Ensure all tests pass
6. Update documentation if necessary
7. Submit a pull request

#### Pull Request Guidelines

- Follow the coding standards (see below)
- Include tests for new features or bug fixes
- Update documentation for significant changes
- Keep pull requests focused on a single concern
- Reference any related issues

## Development Workflow

1. Set up your local development environment following the instructions in the README
2. Create a branch for your work
3. Make your changes
4. Run tests locally
5. Submit a pull request

### Branching Strategy

- `main` - Production-ready code
- `develop` - Integration branch for features
- `feature/[name]` - Feature branches
- `bugfix/[name]` - Bug fix branches

## Coding Standards

### C# Code Style

- Follow Microsoft's [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use C# 12 features appropriately
- Prefer async/await for I/O operations
- Use cancellation tokens for long-running operations
- Implement proper error handling and logging
- Write XML documentation for public APIs

### Architecture Guidelines

- Follow Clean Architecture principles
- Keep domain entities pure without infrastructure dependencies
- Use interface-based design for flexibility
- Implement dependency injection
- Use immutable design where possible
- Follow domain-driven design principles

### Testing Requirements

- Write unit tests for business logic
- Write integration tests for infrastructure components
- Aim for high test coverage of critical paths
- Use meaningful test names that describe the behavior being tested

## Review Process

All submissions require review before being merged:

1. Automated checks must pass (build, tests, linting)
2. At least one maintainer must approve the changes
3. Changes may require revisions before being accepted

## Getting Help

If you need help with your contribution:

- Ask questions in the issue you're working on
- Join our community chat (link to be added)
- Reach out to the maintainers

Thank you for contributing to Fuddy-Duddy! 
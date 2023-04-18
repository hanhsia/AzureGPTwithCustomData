
<!-- Chinese -->
# 开发文档

## 概述

该ASP.NET Core 7.0后端应用为前端提供了Web API，实现了两种方法的GPT聊天，同时提供了Azure Search和OpenAI Embedding两种方法获取私域数据，结合Azure OpenAI的GPT进行问答。支持auzregbt-turbo 3.5, gpt4.0和gpt4.0-32k。


## 开发规范

遵循GitHub上的标准开发规范，包括：

1. 使用有意义的命名规则，避免使用简写和缩写。
2. 保持代码整洁，遵循DRY（Don't Repeat Yourself）原则。
3. 使用适当的注释来解释代码的功能和目的。
4. 保持一致的代码风格和缩进。
5. 使用版本控制（如Git）来管理代码。
6. 编写单元测试以确保代码的正确性和稳定性。
7. 遵循SOLID原则以实现可维护和可扩展的代码。

## 编译步骤

### 使用Visual Studio 2022

1. 打开Visual Studio 2022。
2. 选择“文件”>“打开”>“项目/解决方案”，然后选择解决方案文件（.sln）。
3. 确保在解决方案资源管理器中选中项目，然后选择“生成”>“生成解决方案”以编译项目。
4. 选择“调试”>“开始调试”以运行项目。

### 使用dotnet命令行

1. 打开命令提示符或终端。
2. 导航到项目文件夹（包含.csproj文件的文件夹）。
3. 运行以下命令以编译项目：

```
dotnet build
```

4. 运行以下命令以运行项目：

```
dotnet run
```
<!-- English -->
# Development Document

## Overview

This ASP.NET Core 7.0 backend application provides Web API for the frontend, implementing GPT chat with two methods, and providing two methods for obtaining private domain data through Azure Search and OpenAI Embedding, combined with Azure OpenAI's GPT for Q&A. It supports auzregbt-turbo 3.5, gpt4.0, and gpt4.0-32k.


## Development Standards

Follow the standard development practices on GitHub, including:

1. Use meaningful naming conventions, avoiding abbreviations and acronyms.
2. Keep the code clean and follow the DRY (Don't Repeat Yourself) principle.
3. Use appropriate comments to explain the functionality and purpose of the code.
4. Maintain consistent code style and indentation.
5. Use version control (e.g., Git) to manage the code.
6. Write unit tests to ensure the correctness and stability of the code.
7. Follow the SOLID principles for maintainable and extensible code.

## Compilation Steps

### Using Visual Studio 2022

1. Open Visual Studio 2022.
2. Select "File" > "Open" > "Project/Solution" and then choose the solution file (.sln).
3. Make sure the project is selected in the Solution Explorer, then select "Build" > "Build Solution" to compile the project.
4. Select "Debug" > "Start Debugging" to run the project.

### Using dotnet Command Line

1. Open the command prompt or terminal.
2. Navigate to the project folder (the folder containing the .csproj file).
3. Run the following command to compile the project:

```
dotnet build
```

4. Run the following command to run the project:

```
dotnet run
```
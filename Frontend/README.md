# 前端React程序开发文档
简介
本项目是一个前端React程序，实现了两个不同的bot界面。一个使用bot framework sdk中的direct line连接后端，另一个直接调用asp .net core api。项目中设有一个hard code的用户名密码，用户名: admin，密码: 123456。

开发环境
Node.js
React
TypeScript
项目结构
frontend/
   ├── public/
   ├── src/
   │   ├── components/
   │   ├── pages/
   │   ├── utils/
   │   ├── App.tsx
   │   └── index.tsx
   ├── package.json
   └── tsconfig.json
依赖包
以下是项目中使用的主要依赖包：

@azure/msal-browser: Microsoft Authentication Library (MSAL) for JavaScript browser-based applications
@azure/msal-react: React wrapper for MSAL Browser
@azure/storage-blob: Azure Storage Blob client library for JavaScript
@emotion/react: CSS-in-JS library for styling React components
@emotion/styled: Styled components for Emotion
@fluentui/react: Microsoft Fluent UI React components
@fluentui/react-icons: Fluent UI icons for React
@microsoft/applicationinsights-react-js: React plugin for Application Insights JavaScript SDK
@microsoft/applicationinsights-web: Application Insights JavaScript SDK
@mui/icons-material: Material-UI icons
@mui/material: Material-UI React components
@react-spring/web: React-spring animation library for web
axios: Promise-based HTTP client for the browser and Node.js
botframework-directlinejs: Direct Line client for JavaScript
botframework-webchat: Web Chat component for Bot Framework
dompurify: DOMPurify is a DOM-only, super-fast, uber-tolerant XSS sanitizer for HTML, MathML and SVG
jwt-decode: Decode JWT tokens
react: React library
react-dom: React DOM library
react-dropzone: File dropzone component for React
react-markdown: Markdown component for React
react-quill: Quill rich text editor for React
react-router-dom: DOM bindings for React Router
react-scripts: Scripts and configuration used by Create React App
react-syntax-highlighter: Syntax highlighting component for React
typescript: TypeScript language
运行脚本
在项目根目录下，可以运行以下命令：

npm start
在开发模式下运行应用程序。
打开 http://localhost:3000 在浏览器中查看。

npm test
以交互式监视模式启动测试运行程序。

npm run build
将应用程序构建到 build 文件夹中。
它会在生产模式下正确地打包React，并优化构建以获得最佳性能。

npm run eject
注意: 这是一个单向操作。一旦你 eject，你就不能回去了！

如果你对构建工具和配置选择不满意，可以随时 eject。此命令将从项目中删除单个构建依赖项。

代码规范
项目使用Prettier和ESLint进行代码格式化和规范检查。请确保遵循项目中定义的代码规范。

性能和安全
在开发过程中，请注意检查潜在的性能问题和安全风险。例如，避免在组件中使用内联函数，以减少不必要的重新渲染。对于安全风险，请确保对用户输入进行适当的验证和清理，以防止跨站脚本（XSS）攻击等安全漏洞。

贡献
在为项目做出贡献时，请确保遵循代码规范，并在提交更改之前进行充分的测试。
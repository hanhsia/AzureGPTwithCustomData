
<!-- Chinese -->
本项目演示了如何使用嵌入式和Azure认知服务搭建一个类似GPT的聊天机器人，并利用用户自有数据。该项目展示了两种不同风格的聊天机器人：Bot页面基于Bot Framework SDK，使用Azure认知服务，支持如Teams和Facebook等应用；Chat页面是一个标准的网页聊天应用，使用嵌入式技术搜索用户自有数据。本项目基于以下Azure服务：

Azure OpenAI
Azure Cognitive Search
Azure虚拟机（用于部署qdrant矢量数据库，可在2和3之间选择）
Azure Web应用（可选）
Azure静态应用（可选）
Azure Blob存储服务（可选）
部署完成后，您可以通过网站上传包含用户数据的PDF文档，然后立即与聊天机器人进行交流。以下是本项目的部署文档。关于前端和后端的运行，请分别参阅相应目录中的使用说明。

# 部署文档

本部署文档将指导您完成以下任务：

1. 创建所需的 Azure 服务
2. 安装 Qdrant 矢量数据库
3. 部署 React 前端
4. 安装 .NET 后端

## 1. 创建所需的 Azure 服务

在 Azure 门户中创建以下服务：

- Azure OpenAI 服务
- Azure Search 服务
- Azure Blob 存储服务
- Azure Bot 服务

### a. 创建 Azure OpenAI 服务

1. 登录到 Azure 门户。
2. 单击左侧菜单栏上的 + 创建资源。
3. 在搜索框中输入 "OpenAI" 并选择 OpenAI 服务。
4. 单击 创建。
5. 填写以下信息：
   - 订阅：选择您的 Azure 订阅。
   - 资源组：选择现有资源组或创建新资源组。
   - 名称：输入服务名称。
   - 位置：选择服务所在的区域。
   - 定价层：选择适合您需求的定价层。
6. 单击 查看 + 创建，然后单击 创建。
7. 你需要在Model Deployments中创建以下模型：
   - text-embedding-ada-002
   - gpt-35-turbo
   - text-davinci-003

### b. 创建 Azure Search 服务

1. 登录到 Azure 门户。
2. 单击左侧菜单栏上的 + 创建资源。
3. 在搜索框中输入 "Azure Search" 并选择 Azure Search。
4. 单击 创建。
5. 填写以下信息：
   - 订阅：选择您的 Azure 订阅。
   - 资源组：选择现有资源组或创建新资源组。
   - URL：输入服务的唯一 URL。
   - 位置：选择服务所在的区域。
   - 定价层：选择适合您需求的定价层。
6. 单击 创建。

### c. 创建 Azure Blob 存储服务

1. 登录到 Azure 门户。
2. 单击左侧菜单栏上的 + 创建资源。
3. 在搜索框中输入 "Storage Account" 并选择 Storage Account。
4. 单击 创建。
5. 填写以下信息：
   - 订阅：选择您的 Azure 订阅。
   - 资源组：选择现有资源组或创建新资源组。
   - 存储帐户名称：输入唯一的存储帐户名称。
   - 位置：选择服务所在的区域。
   - 性能：选择适合您需求的性能层。
   - 帐户类型：选择适合您需求的帐户类型。
6. 单击 查看 + 创建，然后单击 创建。

### d. 创建 Azure Bot 服务

1. 登录到 Azure 门户。
2. 单击左侧菜单栏上的 + 创建资源。
3. 在搜索框中输入 "Bot" 并选择Azure Bot。
4. 单击 创建。
5. 填写以下信息：
   - 订阅：选择您的 Azure 订阅。
   - 资源组：选择现有资源组或创建新资源组。
   - 名称：输入 Bot 名称。
   - 位置：选择服务所在的区域。
   - Microsoft App ID：选择 创建新的 Microsoft App ID。
6. 单击 创建。

创建完成后，您需要配置 Bot 服务的设置获取 Microsoft App ID 和密码，并且添加Directline 通道，获得Directline的token和。这些信息将在后续部署步骤中使用。

## 2. 安装 Qdrant 矢量数据库

### a. 在 Azure 上创建一台 Linux 虚拟机

1. 登录到 Azure 门户。
2. 单击左侧菜单栏上的 + 创建资源。
3. 在搜索框中输入 "Linux virtual machine" 并选择 Linux virtual machine。
4. 单击 创建。
5. 填写以下信息：
   - 订阅：选择您的 Azure 订阅。
   - 资源组：选择现有资源组或创建新资源组。
   - 虚拟机名称：输入虚拟机名称。
   - 区域：选择虚拟机所在的区域。
   - 映像：选择所需的 Linux 发行版。
   - 大小：选择适合您需求的虚拟机大小。
   - 管理员帐户：输入管理员用户名和密码或 SSH 公钥。
6. 在 网络 选项卡中，确保为虚拟机分配了公共 IP 地址，并设置 DNS 名称。
7. 在 网络 选项卡中，NIC network security group选择为高级，然后添加一个新的入站端口规则以在 Azure 上打开 6333 端口。
8. 单击 查看 + 创建，然后单击 创建。

### b. 登录该虚拟机并安装 Docker

1. 使用 SSH 连接到您的虚拟机。您可以使用以下命令：
   ```
   ssh <username>@<vm_public_ip>
   ```
   确保将 `<username>` 替换为您的管理员用户名，将 `<vm_public_ip>` 替换为虚拟机的公共 IP 地址。输入密码后进入linux环境。
2. 安装 Docker：
   ```
   curl -fsSL https://get.docker.com -o get-docker.sh
   sudo sh get-docker.sh
   ```

### c. 启动 Docker

```
sudo systemctl start docker
```

### d. 加载 Qdrant 镜像

```
sudo docker pull qdrant/qdrant
```

### e. 运行 Qdrant 容器

1. 在虚拟机上创建一个目录，用于存储 Qdrant 数据，例如/var/lib/qdr：
   ```
   mkdir -p /path/to/data
   ```
2. 运行 Qdrant 容器：
   ```
   sudo docker run -p 6333:6333 -v /path/to/data:/qdrant/storage qdrant/qdrant
   ```

现在，Qdrant 矢量数据库已成功安装并运行在 Azure 虚拟机的 6333 端口上。

如果你想在虚拟机重新启动的时候自动运行，需要创建一个服务，步骤如下：
1. 创建一个新的 Systemd 服务文件：

```bash
sudo nano /etc/systemd/system/qdrant.service
```

2. 将以下内容粘贴到文件中，确保将 `/path/to/data` 替换为实际路径：

```
[Unit]
Description=Qdrant Docker Service
After=docker.service
Requires=docker.service

[Service]
Type=simple
ExecStart=/usr/bin/docker run -p 6333:6333 -v /path/to/data:/qdrant/storage qdrant/qdrant
ExecStop=/usr/bin/docker stop qdrant
Restart=always

[Install]
WantedBy=multi-user.target
```

3. 保存并退出文件。然后运行以下命令以启用并启动服务：

```bash
sudo systemctl enable qdrant.service
sudo systemctl start qdrant.service
```
检查服务状态：

```bash
sudo systemctl status qdrant.service
```

如果服务正在运行，您将看到类似于以下内容的输出：

```
● qdrant.service - Qdrant Docker Service
   Loaded: loaded (/etc/systemd/system/qdrant.service; enabled; vendor preset: enabled)
   Active: active (running) since Mon 2021-11-22 12:34:56 UTC; 1min 5s ago
 Main PID: 12345 (docker)
    Tasks: 15 (limit: 4915)
   CGroup: /system.slice/qdrant.service
           └─12345 /usr/bin/docker run -p 6333:6333 -v /path/to/data:/qdrant/storage qdrant/qdrant
```

## 3. 部署 React 前端

### a. 安装 Node.js

1. 访问 Node.js 官方网站。
2. 下载适合您操作系统的最新 LTS 版本的 Node.js 安装程序。
3. 运行安装程序并按照提示完成 Node.js 安装。

### b. 安装 Vite

1. 打开命令提示符或终端。
2. 导航到您的 React 项目目录：
   ```
   cd /path/to/your/react/project
   ```
3. 安装 Vite 作为开发依赖：
   ```
   npm install vite -D
   ```

### c. 运行 React

- 开发环境：直接运行以下命令：
  ```
  npm run dev
  ```
- 生产环境：执行以下步骤
  1. 构建生产版本：
     ```
     npm run build
     ```
  2. 在 Azure 上创建一台 Windows Server 虚拟机
     - 具有公共 IP
     - 设置 DNS 名称
     - 在 Azure 上打开 80 和 443 端口
     - 如果多个 Web 应用部署在同一台机器，需要在 Azure 上打开端口后远程登录该机器，在 VM 的防火墙里打开对应端口。
  3. 将 dist 目录拷贝到虚拟机上的一个目录（例如：C:\inetpub\wwwroot\your-app）。
  4. 将 web.config 文件也拷贝到这个目录。
  5. 在服务器上安装 IIS，添加 web site，设置物理路径为 dist 目录，启动该 web site。

### d. 配置反向代理（可选）

如果您需要将 React 前端与后端 API 一起部署在同一域名下，您可以在 IIS 中配置反向代理。这需要安装 URL Rewrite 模块和 Application Request Routing 模块。

1. 下载并安装 URL Rewrite 和 Application Request Routing。
2. 在 IIS 管理器中，选择您的站点。
3. 双击 URL 重写。
4. 单击右侧操作窗格中的 添加规则。
5. 选择 反向代理，然后单击 确定。
6. 输入您的后端服务器地址（例如：http://your-backend-server.com），然后单击 确定。

现在，您已成功部署了 React 前端，并根据需要配置了反向代理。

## 4. 安装 .NET 后端

### a. 安装 ASP.NET Core Runtime 7.0

1. 访问 .NET 官方网站。
2. 下载适合您操作系统的 ASP.NET Core Runtime 安装程序。
3. 运行安装程序并按照提示完成 ASP.NET Core Runtime 安装。

### b. 设置 Bot 相关参数

1. 在 Azure Bot 中配置 Messaging endpoint 为 https://<后端服务器名>/api/messages。
2. 从 Azure Bot 中获取的 client_id，secret_key（如果没有的话创建一个）以及 Directline 的 secret 配置到 appsettings.json 的 MicrosoftAppId，MicrosoftAppPassword 和 MicrosoftDirectlineSecret 中。

### c. 配置 appsettings.json 中的 SearchClientSettings，BlobStorageClientSettings 和 AzureOpenAIClientSettings

1. 打开 appsettings.json 文件。
2. 使用从 Azure 门户获取的相关信息更新以下设置：

```json
{
  "SearchClientSettings": {
    "ServiceName": "<your_azure_search_service_name>",
    "ApiKey": "<your_azure_search_api_key>"
  },
  "BlobStorageClientSettings": {
    "ConnectionString": "<your_azure_blob_storage_connection_string>",
    "ContainerName": "<your_azure_blob_storage_container_name>"
  },
  "AzureOpenAIClientSettings": {
    "ApiKey": "<your_azure_openai_api_key>"
  },
  "QdrantEndpoint": "<your_qdrant database address>",
  "MicrosoftAppId": "<your bot application Id>",
  "MicrosoftAppPassword": "<you bot secret",
  "MicrosoftDirectlineSecret": "<directline secret>",
}
```

### d. 运行 .NET 后端

1. 打开命令提示符或终端。
2. 导航到您的 .NET 项目目录：
   ```
   cd /path/to/your/dotnet/project
   ```
3. 开发环境：直接运行以下命令：
   ```
   dotnet run
   ```
4. 生产环境：执行以下命令进行发布：
   ```
   dotnet publish --configuration Release --output ./publish
   ```

### e. 部署到 VM 或 Azure Web 服务

1. 如果部署到 VM，需要将 publish 目录下的内容拷贝到 VM 上，配置 IIS。这里注意需要配置 HTTPS 以及证书。
2. 如果部署到 Azure Web 服务上，可以安装 Azure CLI，将 "publish" 文件夹压缩为 "publish.zip"，然后运行以下命令：
   ```
   az webapp deployment source config-zip --src "<path/to/publish.zip>" --name <MyWebApp> --resource-group <MyResourceGroup>
   ```

现在，您已成功部署了 .NET 后端。

<!-- English -->
This project demonstrates how to build a GPT-like chatbot using Embedding and Azure Cognitive Services with user's own data. The project features two different styles of chatbots: the Bot page is based on the Bot Framework SDK and uses Azure Cognitive Services, supporting applications like Teams and Facebook; the Chat page is a standard web chat application that uses Embedding to search user's own data. This project is based on the following Azure services:

Azure OpenAI
Azure Cognitive Search
Azure Virtual Machine (for deploying qdrant vector database, choose between 2 and 3)
Azure Web App (optional)
Azure Static App (optional)
Azure Blob Storage Service (optional)
After deployment, you can upload PDF documents containing user data through the website, and then chat with the chatbot immediately. Below is the deployment documentation for this project. For the operation of the front-end and back-end, please refer to the usage instructions in the corresponding directories.

# Deployment Document
This deployment document will guide you through the following tasks:
1. Create the required Azure services
2. Install Qdrant vector database
3. Deploy React frontend
4. Install .NET backend

## 1. Create the required Azure services
Create the following services in the Azure portal:
- Azure OpenAI service
- Azure Search service
- Azure Blob storage service
- Azure Bot service

### a. Create Azure OpenAI service
1. Log in to the Azure portal.
2. Click on + Create a resource in the left menu bar.
3. Enter "OpenAI" in the search box and select OpenAI service.
4. Click Create.
5. Fill in the following information:
   - Subscription: Select your Azure subscription.
   - Resource group: Select an existing resource group or create a new one.
   - Name: Enter the service name.
   - Location: Select the region where the service is located.
   - Pricing tier: Select the pricing tier that suits your needs.
6. Click Review + create, then click Create.
7. You need to create the following models in Model Deployments:
   - text-embedding-ada-002
   - gpt-35-turbo
   - text-davinci-003

### b. Create Azure Search service
1. Log in to the Azure portal.
2. Click on + Create a resource in the left menu bar.
3. Enter "Azure Search" in the search box and select Azure Search.
4. Click Create.
5. Fill in the following information:
   - Subscription: Select your Azure subscription.
   - Resource group: Select an existing resource group or create a new one.
   - URL: Enter the unique URL for the service.
   - Location: Select the region where the service is located.
   - Pricing tier: Select the pricing tier that suits your needs.
6. Click Create.

### c. Create Azure Blob storage service
1. Log in to the Azure portal.
2. Click on + Create a resource in the left menu bar.
3. Enter "Storage Account" in the search box and select Storage Account.
4. Click Create.
5. Fill in the following information:
   - Subscription: Select your Azure subscription.
   - Resource group: Select an existing resource group or create a new one.
   - Storage account name: Enter a unique storage account name.
   - Location: Select the region where the service is located.
   - Performance: Select the performance tier that suits your needs.
   - Account type: Select the account type that suits your needs.
6. Click Review + create, then click Create.

### d. Create Azure Bot service
1. Log in to the Azure portal.
2. Click on + Create a resource in the left menu bar.
3. Enter "Bot" in the search box and select Azure Bot.
4. Click Create.
5. Fill in the following information:
   - Subscription: Select your Azure subscription.
   - Resource group: Select an existing resource group or create a new one.
   - Name: Enter the Bot name.
   - Location: Select the region where the service is located.
   - Microsoft App ID: Select Create a new Microsoft App ID.
6. Click Create.
After creation, you need to configure the Bot service settings to obtain the Microsoft App ID and password, and add the Directline channel to obtain the Directline token and secret. These will be used in subsequent deployment steps.

## 2. Install Qdrant vector database
### a. Create a Linux virtual machine on Azure
1. Log in to the Azure portal.
2. Click on + Create a resource in the left menu bar.
3. Enter "Linux virtual machine" in the search box and select Linux virtual machine.
4. Click Create.
5. Fill in the following information:
   - Subscription: Select your Azure subscription.
   - Resource group: Select an existing resource group or create a new one.
   - Virtual machine name: Enter the virtual machine name.
   - Region: Select the region where the virtual machine is located.
   - Image: Select the desired Linux distribution.
   - Size: Select the virtual machine size that suits your needs.
   - Administrator account: Enter the administrator username and password or SSH public key.
6. In the Networking tab, ensure that a public IP address is assigned to the virtual machine and set the DNS name.
7. In the Networking tab, select Advanced for the NIC network security group, and then add a new inbound port rule to open port 6333 on Azure.
8. Click Review + create, then click Create.

### b. Log in to the virtual machine and install Docker
1. Connect to your virtual machine using SSH. You can use the following command:
   ```
   ssh <username>@<vm_public_ip>
   ```
   Make sure to replace `<username>` with your administrator username and `<vm_public_ip>` with the public IP address of the virtual machine. Enter the password to access the Linux environment.
2. Install Docker:
   ```
   curl -fsSL https://get.docker.com -o get-docker.sh
   sudo sh get-docker.sh
   ```

### c. Start Docker
```
sudo systemctl start docker
```

### d. Load Qdrant image
```
sudo docker pull qdrant/qdrant
```

### e. Run Qdrant container
1. Create a directory on the virtual machine to store Qdrant data, such as /var/lib/qdr:
   ```
   mkdir -p /path/to/data
   ```
2. Run the Qdrant container:
   ```
   sudo docker run -p 6333:6333 -v /path/to/data:/qdrant/storage qdrant/qdrant
   ```
Now, the Qdrant vector database is successfully installed and running on port 6333 of the Azure virtual machine.
If you want to automatically run the service when the virtual machine restarts, you need to create a service with the following steps:

1. Create a new Systemd service file:

```bash
sudo nano /etc/systemd/system/qdrant.service
```

2. Paste the following content into the file, making sure to replace `/path/to/data` with the actual path:

```
[Unit]
Description=Qdrant Docker Service
After=docker.service
Requires=docker.service

[Service]
Type=simple
ExecStart=/usr/bin/docker run -p 6333:6333 -v /path/to/data:/qdrant/storage qdrant/qdrant
ExecStop=/usr/bin/docker stop qdrant
Restart=always

[Install]
WantedBy=multi-user.target
```

3. Save and exit the file. Then run the following commands to enable and start the service:

```bash
sudo systemctl enable qdrant.service
sudo systemctl start qdrant.service
```

Check the service status:

```bash
sudo systemctl status qdrant.service
```

If the service is running, you will see output similar to the following:

```
● qdrant.service - Qdrant Docker Service
   Loaded: loaded (/etc/systemd/system/qdrant.service; enabled; vendor preset: enabled)
   Active: active (running) since Mon 2021-11-22 12:34:56 UTC; 1min 5s ago
 Main PID: 12345 (docker)
    Tasks: 15 (limit: 4915)
   CGroup: /system.slice/qdrant.service
           └─12345 /usr/bin/docker run -p 6333:6333 -v /path/to/data:/qdrant/storage qdrant/qdrant
```

## 3. Deploy React frontend
### a. Install Node.js
1. Visit the Node.js official website.
2. Download the latest LTS version of the Node.js installer for your operating system.
3. Run the installer and follow the prompts to complete the Node.js installation.

### b. Install Vite
1. Open the command prompt or terminal.
2. Navigate to your React project directory:
   ```
   cd /path/to/your/react/project
   ```
3. Install Vite as a development dependency:
   ```
   npm install vite -D
   ```

### c. Run React
- Development environment: Run the following command directly:
  ```
  npm run dev
  ```
- Production environment: Perform the following steps
  1. Build the production version:
     ```
     npm run build
     ```
  2. Create a Windows Server virtual machine on Azure
     - With a public IP
     - Set the DNS name
     - Open ports 80 and 443 on Azure
     - If multiple web apps are deployed on the same machine, you need to open the corresponding ports in the VM's firewall after logging in to the machine on Azure.
  3. Copy the dist directory to a directory on the virtual machine (e.g., C:\inetpub\wwwroot\your-app).
  4. Copy the web.config file to this directory as well.
  5. Install IIS on the server, add a web site, set the physical path to the dist directory, and start the web site.

### d. Configure reverse proxy (optional)
If you need to deploy the React frontend and backend API together under the same domain, you can configure a reverse proxy in IIS. This requires installing the URL Rewrite module and the Application Request Routing module.
1. Download and install URL Rewrite and Application Request Routing.
2. In IIS Manager, select your site.
3. Double-click URL Rewrite.
4. Click Add Rule(s) in the right-hand Actions pane.
5. Select Reverse Proxy, then click OK.
6. Enter your backend server address (e.g., http://your-backend-server.com), then click OK.
Now, you have successfully deployed the React frontend and configured the reverse proxy as needed.

## 4. Install .NET backend
### a. Install ASP.NET Core Runtime 7.0
1. Visit the .NET official website.
2. Download the ASP.NET Core Runtime installer for your operating system.
3. Run the installer and follow the prompts to complete the ASP.NET Core Runtime installation.

### b. Set Bot-related parameters
1. Configure the Messaging endpoint in Azure Bot to https://<backend_server_name>/api/messages.
2. Configure the client_id, secret_key (create one if not available), and Directline secret obtained from Azure Bot in appsettings.json's MicrosoftAppId, MicrosoftAppPassword, and MicrosoftDirectlineSecret.

### c. Configure SearchClientSettings, BlobStorageClientSettings, and AzureOpenAIClientSettings in appsettings.json
1. Open the appsettings.json file.
2. Update the following settings with the relevant information obtained from the Azure portal:
   ```
   {
     "SearchClientSettings": {
       "ServiceName": "<your_azure_search_service_name>",
       "ApiKey": "<your_azure_search_api_key>"
     },
     "BlobStorageClientSettings": {
       "ConnectionString": "<your_azure_blob_storage_connection_string>",
       "ContainerName": "<your_azure_blob_storage_container_name>"
     },
     "AzureOpenAIClientSettings": {
       "ApiKey": "<your_azure_openai_api_key>"
     },
   "QdrantEndpoint": "<your_qdrant database address>",
   "MicrosoftAppId": "<your bot application Id>",
       "MicrosoftAppPassword": "<you bot secret",
   "MicrosoftDirectlineSecret": "<directline secret>",
   }
   ```

### d. Run .NET backend
1. Open the command prompt or terminal.
2. Navigate to your .NET project directory:
   ```
   cd /path/to/your/dotnet/project
   ```
3. Development environment: Run the following command directly:
   ```
   dotnet run
   ```
4. Production environment: Execute the following command to publish:
   ```
   dotnet publish --configuration Release --output ./publish
   ```

### e. Deploy to VM or Azure Web Service
1. If deploying to a VM, copy the contents of the publish directory to the VM and configure IIS. Note that you need to configure HTTPS and certificates.
2. If deploying to Azure Web Service, install the Azure CLI, compress the "publish" folder to "publish.zip", and run the following command:
   ```
   az webapp deployment source config-zip --src "<path/to/publish.zip>" --name <MyWebApp> --resource-group <MyResourceGroup>
   ```
Now, you have successfully deployed the .NET backend.


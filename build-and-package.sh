#!/bin/bash
set -e  # If any command fails (exits with a non-zero status), immediately stop the script.

# 0. Dependencies
# sudo npm install -g n
# sudo n latest
# sudo apt install libnss3-tools mkcert docker.io docker-compose docker-buildx

# 1. Setup Certificates
# echo "🏗️  Set up Certificate..."
# mkcert --install
# mkcert -key-file certs/key.pem -cert-file certs/cert.pem httplogger.local

# 2. Adjust /etc/hosts file to add line for httplogger.local
# echo "🔐 Checking hosts entry..."
# if ! grep -q "httplogger.local" /etc/hosts; then
#   echo "🧩 Adding httplogger.local to /etc/hosts"
#   echo "127.0.0.1 httplogger.local" | sudo tee -a /etc/hosts > /dev/null
# fi

# 3. Publish backend
echo "🛠️  Publishing backend..."
dotnet publish HttpLogger.Server/HttpLogger.Server.csproj -c Release -o ./publish

# 4. Build docker image
echo "🐳 Building Docker image..."
DOCKER_BUILDKIT=1 docker-compose build

# 5. Inform user we're ready to go
echo "✅ Ready to run with: docker-compose up"
echo "✅ Terminate with: CTRL+C"
echo "✅ Or run in background with: docker-compose up -d"
echo "✅ Terminate with: docker-compose down"

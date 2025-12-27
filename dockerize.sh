#!/bin/bash
set -e  # If any command fails (exits with a non-zero status), immediately stop the script.

# 1. Dependencies
# sudo npm install -g n
# sudo n latest
# sudo apt install libnss3-tools mkcert docker.io docker-compose-v2 docker-buildx
# sudo usermod -aG docker $USER
# Once you do the last line, logout and relogin, or do: newgrp docker

# 2. Setup Certificates
echo "🏗️  Set up Certificate..."
# mkcert --install   # Adds a CA cert. To remove, do: mkcert uninstall
if [ ! -d certs ]; then
  echo "📁 'certs/' folder not found. Creating and generating cert..."
  mkdir -p certs
  mkcert -key-file certs/key.pem -cert-file certs/cert.pem httplogger.local
else
  echo "✅ 'certs/' folder already exists. Skipping cert generation."
fi

# 3. Make ./data folder, which is mapped to container's /app/data
# If we don't do this and it gets auto-created, it belongs to root, and npm run clean can't delete it
if [ ! -d data ]; then
  mkdir -p data
fi

# 4. Adjust /etc/hosts file to add line for httplogger.local
# echo "🔐 Checking hosts entry..."
# if ! grep -q "httplogger.local" /etc/hosts; then
#   echo "🧩 Adding httplogger.local to /etc/hosts"
#   echo "127.0.0.1 httplogger.local" | sudo tee -a /etc/hosts > /dev/null
# fi

# 5. Publish backend
echo "🛠️  Publishing backend..."
dotnet publish HttpLogger.Server/HttpLogger.Server.csproj -c Release -o ./publish

# 6 Check and remove existing Docker image (if it exists)
echo "🧼 Checking for existing Docker image..."
# Extract image name from docker-compose.yml
IMAGE_NAME=$(grep 'image:' docker-compose.yml | awk '{print $2}')
if docker images --format "{{.Repository}}:{{.Tag}}" | grep -q "^$IMAGE_NAME$"; then
  echo "🗑️  Removing existing image: $IMAGE_NAME"
  docker rmi "$IMAGE_NAME"
else
  echo "✅ No existing image found. Continuing..."
fi

# 7. Build docker image
echo "🐳 Building Docker image..."
DOCKER_BUILDKIT=1 docker compose build

# 7. Inform user we're ready to go
echo "✅ Ready to run with: docker compose up"
echo "✅ Terminate with: CTRL+C"
echo "✅ Or run in background with: docker compose up -d"
echo "✅ Terminate with: docker compose down"

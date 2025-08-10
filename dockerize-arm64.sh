#!/bin/bash
set -e  # Exit immediately if any command fails

# 1. Dependencies (commented out for reference)
# sudo apt install libnss3-tools mkcert docker.io docker-compose docker-buildx
# sudo usermod -aG docker $USER
# After group change: logout & login, or run `newgrp docker`

# 2. Setup Certificates
echo "🏗️  Set up Certificate..."
if [ ! -f certs/cert.pem ] || [ ! -f certs/key.pem ]; then
  echo "📁 Missing certs. Creating with mkcert..."
  mkdir -p certs
  mkcert -key-file certs/key.pem -cert-file certs/cert.pem httplogger.local
else
  echo "✅ Certs already exist. Skipping generation."
fi

# 3. Ensure ./data exists
if [ ! -d data ]; then
  mkdir -p data
fi

# 4. Publish backend for ARM64
echo "🛠️  Publishing backend for ARM64..."
dotnet publish HttpLogger.Server/HttpLogger.Server.csproj \
  -c Release \
  -r linux-arm64 \
  --self-contained false \
  -o ./publish-arm64

# 5. Check and remove existing ARM64 image
echo "🧼 Checking for existing ARM64 Docker image..."
IMAGE_NAME=$(grep 'image:' docker-compose.yml | awk '{print $2}')
IMAGE_NAME_ARM64="${IMAGE_NAME}-arm64"

if docker images --format "{{.Repository}}:{{.Tag}}" | grep -q "^${IMAGE_NAME_ARM64}$"; then
  echo "🗑️  Removing existing image: $IMAGE_NAME_ARM64"
  docker rmi "$IMAGE_NAME_ARM64"
else
  echo "✅ No existing ARM64 image found. Continuing..."
fi

# 6. Reset and create buildx builder
BUILDER_NAME=multiarch-builder
if docker buildx inspect "$BUILDER_NAME" >/dev/null 2>&1; then
  echo "🔄 Removing existing builder: $BUILDER_NAME"
  docker buildx rm "$BUILDER_NAME"
fi

echo "🔧 Creating new builder: $BUILDER_NAME"
docker buildx create --name "$BUILDER_NAME" --use

# 7. Build ARM64 image
echo "🐳 Building ARM64 Docker image..."
docker buildx build \
  --platform linux/arm64 \
  -t "$IMAGE_NAME_ARM64" \
  --load .

# 8. Save image to tarball
echo "💾 Saving image to httpresponder-arm64.tar..."
docker save -o httpresponder-arm64.tar "$IMAGE_NAME_ARM64"

# 9. Cleanup: remove the builder
echo "🧹 Cleaning up buildx builder..."
docker buildx rm "$BUILDER_NAME"

# 10. Done
echo "✅ ARM64 image ready: $IMAGE_NAME_ARM64"
echo "✅ You can load it on a Raspberry Pi using:"
echo "   docker load -i httpresponder-arm64.tar"
echo "   docker run -p 443:443 -v \$(pwd)/data:/app/data --name httpresponder $IMAGE_NAME_ARM64"

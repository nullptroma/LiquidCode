#!/bin/bash

# Script to update namespaces in the LiquidCode project

cd "$(dirname "$0")/LiquidCode"

echo "Updating namespaces..."

# Update Infrastructure/Database/Entities
find Infrastructure/Database/Entities -name "*.cs" -type f -exec sed -i 's/namespace LiquidCode\.Models\.Database/namespace LiquidCode.Infrastructure.Database.Entities/g' {} \;

# Update Infrastructure/Database
sed -i 's/namespace LiquidCode\.Db/namespace LiquidCode.Infrastructure.Database/g' Infrastructure/Database/LiquidDbContext.cs
sed -i 's/namespace LiquidCode\.Db/namespace LiquidCode.Infrastructure.Database/g' Infrastructure/Database/ConnectionStringParser.cs
sed -i 's/using LiquidCode\.Models\.Database/using LiquidCode.Infrastructure.Database.Entities/g' Infrastructure/Database/LiquidDbContext.cs

# Update Domain/Services
find Domain/Services -name "*.cs" -type f -exec sed -i 's/namespace LiquidCode\.Services\.AuthService/namespace LiquidCode.Domain.Services.Authentication/g' {} \;
find Domain/Services -name "*.cs" -type f -exec sed -i 's/namespace LiquidCode\.Services\.MissionService/namespace LiquidCode.Domain.Services.Missions/g' {} \;
find Domain/Services -name "*.cs" -type f -exec sed -i 's/namespace LiquidCode\.Services\.SubmitService/namespace LiquidCode.Domain.Services.Submits/g' {} \;

# Update Domain/Repositories
find Domain/Repositories -name "*.cs" -type f -exec sed -i 's/namespace LiquidCode\.Repositories/namespace LiquidCode.Domain.Repositories/g' {} \;

# Update Infrastructure/External
find Infrastructure/External -name "*.cs" -type f -exec sed -i 's/namespace LiquidCode\.Services\.S3ClientService/namespace LiquidCode.Infrastructure.External.S3/g' {} \;
find Infrastructure/External -name "*.cs" -type f -exec sed -i 's/namespace LiquidCode\.Services\.TestingModuleHttpClient/namespace LiquidCode.Infrastructure.External.TestingModule/g' {} \;
find Infrastructure/External -name "*.cs" -type f -exec sed -i 's/namespace LiquidCode\.Services/namespace LiquidCode.Infrastructure.External.S3/g' {} \;

# Update Shared
find Shared -name "*.cs" -type f -exec sed -i 's/namespace LiquidCode\.Models\.Constants/namespace LiquidCode.Shared.Constants/g' {} \;
find Shared -name "*.cs" -type f -exec sed -i 's/namespace LiquidCode\.Extensions/namespace LiquidCode.Shared.Extensions/g' {} \;
find Shared -name "*.cs" -type f -exec sed -i 's/namespace LiquidCode\.Middleware/namespace LiquidCode.Shared.Middleware/g' {} \;

# Update using statements in all new files
find Domain -name "*.cs" -type f -exec sed -i 's/using LiquidCode\.Models\.Database/using LiquidCode.Infrastructure.Database.Entities/g' {} \;
find Domain -name "*.cs" -type f -exec sed -i 's/using LiquidCode\.Models\.Constants/using LiquidCode.Shared.Constants/g' {} \;
find Domain -name "*.cs" -type f -exec sed -i 's/using LiquidCode\.Repositories/using LiquidCode.Domain.Repositories/g' {} \;
find Domain -name "*.cs" -type f -exec sed -i 's/using LiquidCode\.Extensions/using LiquidCode.Shared.Extensions/g' {} \;
find Domain -name "*.cs" -type f -exec sed -i 's/using LiquidCode\.Tools/using LiquidCode.Shared.Tools/g' {} \;

find Infrastructure -name "*.cs" -type f -exec sed -i 's/using LiquidCode\.Models\.Constants/using LiquidCode.Shared.Constants/g' {} \;
find Infrastructure -name "*.cs" -type f -exec sed -i 's/using LiquidCode\.Models\.Database/using LiquidCode.Infrastructure.Database.Entities/g' {} \;

find Shared -name "*.cs" -type f -exec sed -i 's/using LiquidCode\.Models\.Constants/using LiquidCode.Shared.Constants/g' {} \;

echo "Namespaces updated successfully!"

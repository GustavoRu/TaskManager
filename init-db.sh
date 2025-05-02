#!/bin/bash

# Esperar a que SQL Server esté disponible
echo "Esperando a que SQL Server esté disponible..."
sleep 10

# Ejecutar migraciones
echo "Ejecutando migraciones..."
cd src/TaskManager.API
dotnet ef database update

# Si las migraciones fueron exitosas
if [ $? -eq 0 ]; then
    echo "Migraciones aplicadas exitosamente"
else
    echo "Error al aplicar las migraciones"
    exit 1
fi
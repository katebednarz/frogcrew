# Start MySQL database server container (for mac at least)
1. Open terminal, navigate to the docker folder.
2. run 'docker compose up -d'

# Stop the MySQL database server container (for mac)
1. Open terminal, navigate to the docker folder.
2. run 'docker compose down'

# Resetting the database
The data is configured to build from scratch everytime you compose the container. This will reset table structure and data.
" docker compose down && docker compose up -d " -> will restart the container 

# Connection Info
Host: localhost
Port: 3306
User: root
Pass: password
#!/bin/bash

docker run --name docker-pg -p 5432:5432 -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=qwef -e POSTGRES_DB=dev-db -d postgres


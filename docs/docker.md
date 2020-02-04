# Docker

## Useful command
* remove all containers including its volumes use
    ```
    docker rm -vf $(docker ps -a -q)
    ```
* remove all images
    ```
    docker rmi -f $(docker images -a -q)
    ```
* remove unused images
    ```
    docker rmi $(docker images --filter "dangling=true" -q --no-trunc)
    ```
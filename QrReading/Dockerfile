FROM mcr.microsoft.com/azure-functions/python:4-python3.10

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY requirements.txt /
RUN pip install -r /requirements.txt
RUN apt-get install -y openjdk-11-jdk libzbar0 \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

COPY . /home/site/wwwroot
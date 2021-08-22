# tadmor
discord and telegram bot

## local usage
1. install .net 5
2. edit `appsettings.json` or use user secrets to set the values (recommended)
3. run

## CI/CD
this doesn't need to be hosted on windows, but the included workflow targets it

1. install an ssh server on the windows host
2. add secrets to your repository to populate the variables in `.github/workflows/deploy.yaml`

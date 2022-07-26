# Release Checklist

* [ ] Update [deploy-base.md](../deploy-base.md) to reflect the newest version.
* [ ] Update [deploy-demo.md](../deploy-demo.md) to reflect the newest version.
* [ ] Create a new aka.ms url to point to the next release (e.g. )
* [ ] Update the [newdeploymenttemp.json](../deployment/infra/newdeploymenttemp.json) with the new aka.ms url.
* [ ] Create a git tag with pattern X.Y.Z where X, Y, and Z follow the [semver pattern](https://semver.org/). Then push the tag to the origin git repo (github).
    * ```bash
      git tag X.Y.Z
      git push origin --tags
      ```
    * This will trigger the github action
* [ ] Allow the Github action to deploy to the integration environment.
* [ ] Confirm the tests pass on the integration environment.
    * If the tests fail, you can remove the tag from your local and github repo using:
    ```bash
    git push origin --delete X.Y.Z # Delete on Github
    git tag -d X.Y.Z # Delete locally
    ```
    * Fix the errors and then repeat the steps above to recreate the tag locally and push to Github to restart the process.
* [ ] Allow the Github action to deploy to the production (release) environment.
    * This will generate a release that includes the function zip file.
* [ ] Update the newly created aka.ms url to point to the new function zip file available on the release.
* [ ] Add release notes.
* If creating a new release for major or minor version:
    * [ ] Create a new release branch with the last commit and name it 'release/X.Y`
    * [ ] [Update the default branch](https://docs.github.com/en/organizations/managing-organization-settings/managing-the-default-branch-name-for-repositories-in-your-organization) on the github rep to the new release branch.


## Hotfix checklist

* [ ] Create a new branch named `hotfix/nameOfFix`
* [ ] Create a PR for each affected release branch from your hotfix branch.
* [ ] Apply the PR to each affected release.
* If the hotfix affects the Azure Function code:
    * [ ] Create a release with an updated Patch value (e.g. going from 1.1.0 to 1.1.1)
    * [ ] Manually add the FunctionZip.zip file to the release.
    * [ ] Update the related aka.ms url to point to the latest, fixed version.

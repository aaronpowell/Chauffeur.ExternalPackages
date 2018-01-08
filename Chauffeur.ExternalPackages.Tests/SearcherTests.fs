module ``Unit tests for then Search Module``

open Xunit
// open FsUnit.Xunit

open Searcher
open System

type ``Successful Searching``() =
    [<Fact>]
    member x.``Selection will return appropriate ID``() =
        let testId = "5405a34d-752b-4bcf-b356-4c7c60168902"
        async {
            let packages = UmbracoPackages.Parse(sprintf """{
                              "packages": [
                                {
                                  "created": "2012-12-17 20:40:14",
                                  "id": "%s",
                                  "name": "Demo Package",
                                  "excerpt": " Demo package description",
                                  "icon": "",
                                  "url": "",
                                  "downloads": 0,
                                  "likes": 0,
                                  "category": "Developer tools",
                                  "latestVersion": "1.0.0",
                                  "ownerInfo": {
                                    "owner": "Demo",
                                    "ownerAvatar": "",
                                    "contributors": null,
                                    "karma": 0
                                  },
                                  "minimumVersion": "7.5.0",
                                  "score": 0
                                }
                              ]
                            }"""
                            testId
                        )

            let readLineAsync = async { return "1" }
            let writeLineAsync s = async { return () }
            let writeAsync s = async { return () }

            let! selection = displaySearchResults readLineAsync writeLineAsync writeAsync packages.Packages

            match selection with
            | Some value-> Assert.Equal ((Guid.Parse testId), value)
            | None -> Assert.False(true)

            } |> Async.RunSynchronously
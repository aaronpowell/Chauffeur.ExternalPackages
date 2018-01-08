module ``Unit tests for then Search Module``

open Xunit
open Searcher
open System
open FsUnit

type ``Successful Searching``() =
    [<Fact>]
    member __.``Selection will return appropriate ID``() =
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
            let writeLineAsync _ = async { return () }
            let writeAsync _ = async { return () }

            let! selection = displaySearchResults readLineAsync writeLineAsync writeAsync packages.Packages

            match selection with
            | Some value -> value |> should equal (Guid.Parse testId)
            | None -> false |> should be True

            }

    [<Fact>]
    member __.``Selecting cancel returns None``() =
        async {
            let packages = UmbracoPackages.Parse("""{
                              "packages": [
                                {
                                  "created": "2012-12-17 20:40:14",
                                  "id": "5405a34d-752b-4bcf-b356-4c7c60168902",
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
                        )

            let readLineAsync = async { return "q" }
            let writeLineAsync _ = async { return () }
            let writeAsync _ = async { return () }

            let! selection = displaySearchResults readLineAsync writeLineAsync writeAsync packages.Packages

            match selection with
            | Some _ -> false |> should be True
            | None -> true |> should be True
        }
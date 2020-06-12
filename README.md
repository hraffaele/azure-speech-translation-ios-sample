---
page_type: sample
languages:
- csharp
- xamarin forms
products:
- dotnet
- azure cognitive services - Speech Translation
description: "This is a Speech Translation sample application for iOS"
---
# Azure Speech Translation Sample
<!-- 
# Official Microsoft Sample

Guidelines on README format: https://review.docs.microsoft.com/help/onboard/admin/samples/concepts/readme-template?branch=master

Guidance on onboarding samples to docs.microsoft.com/samples: https://review.docs.microsoft.com/help/onboard/admin/samples/process/onboarding?branch=master

Taxonomies for products and languages: https://review.docs.microsoft.com/new-hope/information-architecture/metadata/taxonomies?branch=master
-->

This is an Azure Speech Translation sample application for iOS. It allows you to hold a conversation in two languages with both text and speech translation.

## Contents

Outline the file contents of the repository. It helps users navigate the codebase, build configuration and any related assets.

| File/folder       | Description                                |
|-------------------|--------------------------------------------|
| `App1`            | Sample source code.                        |
| `.gitignore`      | Define what to ignore at commit time.      |
| `README.md`       | This README file.                          |


## Prerequisites

An Azure subscription with a Speech Services and a Text Translation Services subscription is required.

## Setup

Azure subscription API keys must be specified in App1/appsettings.json.

SubscriptionKey must be set to the SpeechServices key.
TextToTextSubscriptionKey must be set to the TextTranslation key.

Additional configuration can be specified in appsettings.json.

<!--
## Running the sample

Outline step-by-step instructions to execute the sample and see its output. Include steps for executing the sample from the IDE, starting specific services in the Azure portal or anything related to the overall launch of the code.

## Key concepts

Provide users with more context on the tools and services used in the sample. Explain some of the code that is being used and how services interact with each other.

-->
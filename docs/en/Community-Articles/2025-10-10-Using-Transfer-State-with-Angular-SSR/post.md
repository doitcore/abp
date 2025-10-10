# From Server to Browser — the Elegant Way: Angular TransferState Explained

## Introduction

When building Angular applications with Server-Side Rendering (SSR), one of the most common performance pitfalls is duplicated data fetching. Server fetch data while generating initial HTML, and then browser bootstraps Angular and it fetches the same data all over again. That’s wasteful, especially for APIs that don’t change often.
To solve this problem, Angular provide **TransferState**. It allows to transfer data from application on server to application on browser, in this way you can avoid redundant unnecessary operations.
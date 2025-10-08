# From Server to Browser — the Elegant Way: Angular TransferState Explained

## Introduction

When building Angular applications with Server-Side Rendering (SSR), one of the most common performance pitfalls is duplicated data fetching. Your server fetches data to render the initial HTML, and then — as soon as the browser bootstraps Angular — it fetches the same data all over again. That’s wasteful, especially for APIs that don’t change often.

Enter TransferState, Angular’s elegant solution to bridge the data gap between the server and the browser. It allows you to transfer pre-fetched data from the server-rendered page to the client seamlessly — no redundant HTTP calls, no flickering spinners, and no extra waiting time.
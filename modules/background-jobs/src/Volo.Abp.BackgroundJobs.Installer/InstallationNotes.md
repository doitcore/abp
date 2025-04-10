# Installation Notes for Background Jobs Module

Background jobs are used to queue some tasks to be executed in the background. You may need background jobs for several reasons. Here are some examples:

- To perform **long-running tasks** without having the users wait. For example, a user presses a 'report' button to start a long-running reporting job. You add this job to the **queue** and send the report's result to your user via email when it's completed.
- To create **re-trying** and **persistent tasks** to **guarantee** that a code will be **successfully executed**. For example, you can send emails in a background job to overcome **temporary failures** and **guarantee** that it eventually will be sent. That way users do not wait while sending emails.

Background jobs are **persistent** that means they will be **re-tried** and **executed** later even if your application crashes.

## Documentation

For detailed information and usage instructions, please visit the [Background Jobs documentation](https://abp.io/docs/latest/framework/infrastructure/background-jobs). 
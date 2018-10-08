# How to contribute

The best way to contribute is to first open an issue to discuss.<br/>
Before doing so, it can also be wise to first having a look at already created issues (opened or closed).

If you want to fix a bug, first describe the bug the best way you can, maybe it has already been fixed, or being fixed.

If you want to implement a new feature, first describe what and how, maybe it is already being implemented by someone else, and the code being reworked that would make it impossible to implement at the moment.

There is nothing more frustrating, deceiving and sad for a developer than seeing your hard work going to trash. I know that for having experiencing it, and I wish that to nobody.

That's why I want people to first discuss about their idea, in order to avoid that to happen as much as possible.

I'm aware not everyone is fluent and comfortable with the English language, but please make an effort for your explanation to be comprehensive, and by all means avoid automatic translation.

## Discuss, explain, describe

The first step prior to any change proposal is to open an issue, and to describe the actual situation and the desired situation, the best and most accurate way you can. Please keep in mind that what is obvious for you might not be for others.

Since each cases are kind of specific (bug fix, new feature, etc...), please describe the actual situation and desired situation accordingly.

Upon discussions and mutual agreement, you may proceed to the next step.

## Submitting changes

Once the topic as been discussed and covered enough, you may proceed to write code and then submit a pull request.

Your pull request should contain as many description as possible, descriptions being the clearer possible. Write your pull request description as if the reviewer as never read the original posts. Do not re-explain everything but link the required conversations to give context to your explanations.

Do not squash your commits before making your pull request, and avoid changing the content of the pull request after submitting it. Tracking changes during review is just a nightmare. If you need to make changes, make sure to discuss about it and get the reviewer's agreement before doing so. Also, please make sure all your commits are self-contained and that a feature does not span several commits, meaning that checking out any commit should result in the application still being working. Achieving this is not always possible so this is not a hard rule, but please make effort to make it this way as much as you can.

## Testing

There is no unit test for the moment, unfortunately, and this is very bad.

I encourage you to add unit tests if you can, or at worst to test you changes one hundred times if necessary. There is nothing worst than checking out code, pressing F5 and getting a `NullReferenceException` right away, incontestable proof that the developer wrote code and click `commit + push`, or tested his/her code, then made a last minute change and didn't retest.

## Coding conventions

Please respect the coding conventions in place. There is no document that stats any of them, but having a look at existing code should answer most of the questions regarding this topic.

- Use spaces, no tabs
- 4 spaces for 1 level of indentation
- Respect C# code conventions and Visual Studio formatting rules (Pascal casing, spacings, etc...)
- Respect MVVM principles as much as possible
- Avoid useless comments (such as `constructor` on a constructor) but comment code that may be hard to understand

I just can't enumerate all rules here, some may be added as specific cases are encountered. In doubt, use common sense, and simply open an issue to ask.

The code base can be dirty in some places, that doesn't mean you have to do the same ;)

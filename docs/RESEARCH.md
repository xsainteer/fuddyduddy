# Ollama

## Concurrency topic

According to [https://github.com/ollama/ollama/issues/358#issuecomment-2092047353](https://github.com/ollama/ollama/issues/358#issuecomment-2092047353)

> @dhiltgen
> I install ollama on Mac.
> are these the default value for this two env?OLLAMA_NUM_PARALLEL=4 OLLAMA_MAX_LOADED_MODELS=4.
> the following should work, right? :)
> ```
> t@603e5f4a42f1 ~ % launchctl setenv OLLAMA_NUM_PARALLEL 5
> t@603e5f4a42f1 ~ % launchctl setenv OLLAMA_MAX_LOADED_MODELS 5
> ```
> to check the value of ENVs :)
> ```
> launchctl getenv OLLAMA_NUM_PARALLEL
> launchctl getenv OLLAMA_MAX_LOADED_MODELS
> ```
> restart ollama to activate new value of ENVs. :) very nice!


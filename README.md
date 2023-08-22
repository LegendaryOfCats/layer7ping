# layer7ping
This tool is "paping-like" but for HTTP

![l7ping](https://cdn.networklayer.net/images/l7ping.png)


# Command Line Parameters

```
l7ping <URL> [HTTP METHOD]
l7ping (http(s)://)duckduckgo.com (get/head)
```

## Example usage

```
> l7ping duckduckgo.com
layer7ping v1.2 - Copyright (c) 2023 NetworkLayer

Establishing connection to https://duckduckgo.com using GET

Connected to https://duckduckgo.com: status=200/OK method=GET time=233ms bytes=81491
Connected to https://duckduckgo.com: status=200/OK method=GET time=158ms bytes=81491
Connected to https://duckduckgo.com: status=200/OK method=GET time=151ms bytes=81491
Connected to https://duckduckgo.com: status=200/OK method=GET time=197ms bytes=81491
Ping statistics for https://duckduckgo.com
    OK: Count = 4, Percentage = 100.00%

```

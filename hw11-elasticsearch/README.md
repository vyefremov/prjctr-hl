# Homework: Elasticsearch

## Task

1. Create index that will serve autocomplete needs with leveraging typos and errors (max 3 typos if word length is bigger than 7).
2. Use English vocabulary as a source of words.

## Solution

It's relatively easy to implement autocomplete with Elasticsearch allowing up to 2 typos:
1. Create an index with a custom analyzer that uses `sandard` tokenizer and `edge_ngram` token filter.
2. Then use `match` query with `fuzziness` parameter set to `auto` to allow up to `2` typos depending on the input length.

However, the task requires to allow up to `3` typos if the word length is bigger than `7`. This is not possible with the built-in `fuzziness` parameter. To achieve this, we need to use:
1. NGram tokenizer to split words into n-grams
2. Custom NGram analyzer to process the input
3. Dynamic calculation of the `minimum_should_match` parameter based on the input length
4. Custom script to filter out results with `Length` less than the input length

#### Setup:
1. Docker Compose — [docker-compose.yml](./docker-compose.yml)
2. Index creation — [IndexManager.cs](./src/HW.Elasticsearch/HW.Elasticsearch/IndexManager.cs)
3. Index bulk initialization — [IndexInitializer.cs](./src/HW.Elasticsearch/HW.Elasticsearch/IndexInitializer.cs)
4. Console application (with querying) — [Program.cs](./src/HW.Elasticsearch/HW.Elasticsearch/Program.cs)

### Query when word length is less than or equal to 7

```json
{
  "query": {
    "match": {
      "word.autocomplete": {
        "query": "vladislav",
        "fuzziness": "auto"
      }
    }
  }
}
```

### Query when word length is bigger than 7

```json
{
  "query": {
    "bool": {
      "should": [
        {
          "match": {
            "Value.ngram": {
              "query": "vladislav",
              "analyzer": "ngram_analyzer",
              "minimum_should_match": "60%"
            }
          }
        }
      ],
      "must": [
        {
          "script": {
            "script": {
              "source": "doc['Value.keyword'].value.length() >= 9",
              "lang": "painless"
            }
          }
        }
      ]
    }
  }
}
```

### Index setup

```json
{
   "mappings":{
      "properties":{
         "Value":{
            "fields":{
               "keyword":{
                  "type":"keyword"
               },
               "autocomplete":{
                  "analyzer":"autocomplete_analyzer",
                  "search_analyzer":"search_analyzer",
                  "type":"text"
               },
               "ngram":{
                  "analyzer":"ngram_analyzer",
                  "search_analyzer":"search_analyzer",
                  "type":"text"
               }
            },
            "type":"text"
         }
      }
   },
   "settings":{
      "analysis":{
         "analyzer":{
            "autocomplete_analyzer":{
               "filter":[
                  "lowercase",
                  "edge_ngram_filter"
               ],
               "tokenizer":"standard",
               "type":"custom"
            },
            "ngram_analyzer":{
               "filter":[
                  "lowercase"
               ],
               "tokenizer":"ngram_tokenizer",
               "type":"custom"
            },
            "search_analyzer":{
               "filter":[
                  "lowercase"
               ],
               "tokenizer":"standard",
               "type":"custom"
            }
         },
         "filter":{
            "edge_ngram_filter":{
               "max_gram":20,
               "min_gram":1,
               "type":"edge_ngram"
            }
         },
         "tokenizer":{
            "ngram_tokenizer":{
               "max_gram":3,
               "min_gram":1,
               "token_chars":[
                  "letter",
                  "digit"
               ],
               "type":"ngram"
            }
         }
      },
      "max_ngram_diff":2,
      "refresh_interval":"10s"
   }
}
```

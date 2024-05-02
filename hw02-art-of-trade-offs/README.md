# HSA L2 Homework: Art of Trade-offs. ATAM

## Task

- System: Hotline
- Scope: Ranking Algorithm
- Quality Attributes: Impact on revenue, Performance, Usability

## Identify Architectural Approaches

- **Personalization algorithm** - predicting user preferences based on past behavior or product attributes;
- **Popularity algorithm** - recommending products based on their overall popularity;
- **Rule-based algorithm** - prioritize products based on pre-defined criteria like profit margin, new arrivals, or seasonal relevance;
- **Context aware algorithm** - takes into account the user's current context (e.g., time of day, location, device used) to suggest relevant products. For example, it might recommend winter clothing during the cold season on a mobile device;
- **Deep learning** -  advanced technique uses artificial neural networks to analyze vast amounts of data and make complex recommendations based on user behavior, product features, and external factors like market trends.

## Generate quality attribute utility tree

### Ranking Utility

1. **Efficiency** - how well the products are offered to user
   - **Impact on revenue** - how easy achieve positive impact on revenue
     - **(H, M)** More than 5% users buying from the home page
   - **Relevancy** - how relevant products are for users
     - **(M, H)** Users satisfactory rate is greater than 80%
2. **Adaptability** - how easy to modify for emerging use cases
   - **Configurability** - how easy to configure through admin panel/no code
     - **(H, M)** Most of the business rules can be configured with no code
   - **Extensibility** - how easy to extend with different data providers or product types
     - **(M, L)** Adding new data provider should take less that 2 weeks of team work
3. **Performance** - how fast ranking is, how fast the main page loads
   - **Response time** - how fast can get list of relevant products
     - **(M, H)** Relevant products needs to be returned in less than 300ms
   - **Scalability** - how easy to scale up for special events
     - **(H, H)** Need to have an ability to scale horizontally (by adding more nodes/pods)
4. **Security**
   - **Fairness** - algorithm is hard to trick
     - **(M, M)** There is no possibility to influence algorithm by malicious actions
   - **Data Privacy** - algorithm protects user data
     - **(M, L)** No one can access other user's recommendations or personal data

> L - Low, M - Medium, H - High
> 
> Pair: (most important, most difficult to achieve)

## Analyze architectural approaches

> Scenario: User opens the homepage. The system needs to provide best recommendations for the user.

### Personalization algorithm

- **Efficiency** - is high when the user is a returning customer, but low for new users;
- **Adaptability** - can be hard to configure and extend, as it requires a lot of data and training;
- **Performance** - is high when the model is trained and cached, but low when it needs to be retrained;
- **Security** - can be hard to ensure fairness and data privacy, as the model can be biased or leak user data.

### Popularity algorithm

- **Efficiency** - is high when the products are popular, but low when the user has unique preferences;
- **Adaptability** - is hard to configure and extend, as it there is no much to configure;
- **Performance** - is high as the popularity data is easy to sort and filter;
- **Security** - can be hard to ensure fairness and data privacy, as the model can be biased (by clickers or bots).

### Rule-based algorithm

- **Efficiency** - is high when the rules are simple and clear, but low when they are complex and hard to understand;
- **Adaptability** - is easy to configure and extend, as it only requires rule changes;
- **Performance** - is high as the rules are easy to apply and test;
- **Security** - is mostly secure, as the rules are transparent and can be audited but can be biased when the rules are exposed.

### Context aware algorithm

- **Efficiency** - is high when the context is matched with user preferences, but mostly medium as it not fully personalized;
- **Adaptability** - is medium as it requires context data which is limited;
- **Performance** - is high as the context data is easy to filter and sort;
- **Security** - is mostly secure, as the context data is not sensitive but can be biased when the context is not relevant.

### Deep learning

- **Efficiency** - is high when the model is trained well but may be low when the model is not trained properly;
- **Adaptability** - is hard to configure and extend, as it requires a lot of data and training;
- **Performance** - is high when the model is trained and cached, but low when it needs to be retrained;
- **Security** - can be hard to ensure fairness and data privacy, as the model can be biased or leak user data.

## Conclusion

| Criteria     | 1st             | 2nd             | 3rd             | 4th             | 5th           |
|--------------|-----------------|-----------------|-----------------|-----------------|---------------|
| Efficiency   | Personalization | Deep learning   | Context aware   | Popularity      | Rule-based    |
| Adaptability | Rule-based      | Personalization | Deep learning   | Context aware   | Popularity    |  
| Performance  | Popularity      | Rule-based      | Personalization | Context aware   | Deep learning |
| Security     | Rule-based      | Context aware   | Popularity      | Personalization | Deep learning |

The best approach is the combination of **Personalization** and **Rule-based** algorithms.
It provides the best efficiency, adaptability, and security.
The trade-off is the performance, which is not the best but still acceptable.
Also, the **Rule-based** algorithm can be used as a fallback when the **Personalization** algorithm fails to provide recommendations.

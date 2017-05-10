**Faross**

*Description*

Faross is a light-weight, easy to configure, monitoring web application for web services/sites. 

*Code design guidelines*

- Models
    - Must be stateless; this enables thread safety and can also save instantiations
    - Constructors should enforce all data/parameter validations
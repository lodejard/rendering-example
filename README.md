

# Principles

* An internal DSL instead of an external DSL

* a yaml or json template file is itself always valid yaml or valid json (editor friendly, supports reformat commands and does not cause extra warnings)

* expression are surrounded by "{{ ... }}" and always fully contained in a yaml or json property name or value string

* directives are surrounded by "{{# ... }} and always fully contained in a yaml or json property name

* expressions and directive syntax is always JMESPath (the same query language used by every Azure CLI command in the --query switch)


# Details

* when property name or value contain JMESPath query encased in "{{ expression }}" 
  * the expression is evaluated and the result becomes the output property name or value

* a property name may contain a directive when encased in "{{# directive expression}}"
  * directives properties disappear from the output

* directives include
    * {{# each *array-expression* }}
    * {{# if *bool-expression* }}
    * {{# elseif *bool-expression* }}
    * {{# else }}
    * {{# file *string-expression* }}

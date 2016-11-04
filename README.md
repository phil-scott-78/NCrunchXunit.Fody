# NCrunchXunit.Fody
Fody add-in to try and get NCrunch to follow xUnit's parallization strategies

By default NCrunch will run all tests in parallel with each other. This applies 
the `ExclusivelyUses` attribute to all classes with the class name forcing each
test collection to respect xUnit's behavior. If a collection attribute is found
it will use that as the token passed to `ExclusivelyUses`
// To enable parallel test execution for the entire assembly (project),
// set the level of parallelism and the scope.
// Workers = 0 means the degree of parallelism will be dynamic based on the number of available processors.
// Scope = ExecutionScope.MethodLevel means tests will be parallelized at the method level.
[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]

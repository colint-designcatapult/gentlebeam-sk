SET workingDir=%cd%

cd ../../databases
git log --pretty=format:%%h -n 1 > %workingDir%/databases_actual_commit.txt
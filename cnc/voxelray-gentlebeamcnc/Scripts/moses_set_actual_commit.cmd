SET workingDir=%cd%

cd ../../moses
git log --pretty=format:%%h -n 1 > %workingDir%/moses_actual_commit.txt
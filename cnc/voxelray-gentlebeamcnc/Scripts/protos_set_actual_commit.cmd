SET workingDir=%cd%

cd ../../protos
git log --pretty=format:%%h -n 1 > %workingDir%/protos_actual_commit.txt
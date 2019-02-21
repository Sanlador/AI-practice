import numpy as np

#reads file of sudoku puzzles and places them in arrays arranged by difficulty (file is specifically formatted prior to input)
def readPuzzleFile(file):
	easy = []
	medium = []
	hard = []
	evil = []

	difficulty = {
		"Easy": 1,
		"Medium": 2,
		"Hard": 3,
		"Evil": 4
	}


	#for each configuration in the text file:
	f = open(file, "r")
	file = f.read().splitlines()
	board = np.zeros((9,9))
	control = False
	i = 0

	for line in file:
		#determine which difficulty the puzzle is
		if len(line.split()) > 1:
			#place the board into the correct array
			if control == True:
				if difficulty[d] == 1:
					easy.append(board)
				elif difficulty[d] == 2:
					medium.append(board)
				elif difficulty[d] == 3:
					hard.append(board)
				elif difficulty[d] == 4:
					evil.append(board)
			control = True
			#throw out first string and read the second
			i = 0
			d = line.split()[1]
			board = np.zeros((9,9))
		#read the following nine lines in as a 9x9 array of ints (the sudoku board)
		else:
			for j in range(9):
				board[i][j] = int(line[j])
			i += 1
			if i == 9:
				i = 0
	return easy, medium, hard, evil

def alldiff(board, row, col):
	if board[row][col] != 0:
		return [board[row][col]]

	if row < 3:
		squareRow = 0
	elif row < 6:
		squareRow = 3
	else:
		squareRow = 6
	if col < 3:
		squareCol = 0
	elif col < 6:
		squareCol = 3
	else:
		squareCol = 6

	domain = [1,2,3,4,5,6,7,8,9]
	for i in range(9):
		if board[row][i] in domain and board[row][i] != 0 and i != col:
			domain.remove(board[row][i])
		if board[i][col] in domain and board[i][col] != 0 and i != row:
			domain.remove(board[i][col])
	for i in range(3):
		for j in range(3):
			if board[squareRow + i][squareCol + j] in domain and board[squareRow + i][squareCol + j] != 0:
				domain.remove(board[squareRow + i][squareCol + j])
	return domain


def success(board):
	#check that board is filled
	for i in range(9):
		for j in range(9):
			if board[i][j] == 0:
				return False
	#check that board has
	for i in range(9):
		if allDiffLine(board, "col", i) == False:
			return False
		if allDiffLine(board, "row", i) == False:
			return False
	for i in range(3):
		for j in range(3):
			if allDiffSquare(board, i, j) == False:
				return False
	return True

class assignment:
	domain = []
	board = []
	def __init__(self, board):
		self.board = board
		for i in range(9):
			self.domain.append([[],[],[],[],[],[],[],[],[]])
			for j in range(9):
				self.domain[i][j] = alldiff(board, i, j)

	def makeAssignment(self, row, col, assignment):
		self.board[row][col] = assignment
		self.domain[row][col] = [0]

	def assignmentComplete(self):
		for i in range(9):
			for j in range(9):
				if (self.domain[i][j]) != [0]:
					return False
		return True

	def nakedSingle(self):
		control = False
		for i in range(9):
			for j in range(9):
				if len(self.domain[i][j]) == 1 and self.domain[i][j] != [0]:
					self.makeAssignment(i,j, self.domain[i][j][0])
					control = True
		return control

	def hiddenSingle(self):
		hidden = False
		for i in range(9):
			for j in range(9):
				#determine square
				if i < 3:
					squareRow = 0
				elif i < 6:
					squareRow = 3
				else:
					squareRow = 6
				if j < 3:
					squareCol = 0
				elif j < 6:
					squareCol = 3
				else:
					squareCol = 6

				control = False
				for d in self.domain[i][j]:
					controlRow = True
					controlCol = True
					controlBox = True
					for x in range(9):
						if control == False:
							if d in self.domain[i][x] and x != j:
								controlRow = False
							if d in self.domain[x][j] and x != i:
								controlCol = False
					for x in range(3):
						for y in range(3):
							if d in self.domain[squareCol + x][squareRow + y]:
								controlBox = False

					if controlRow and controlCol and controlBox:
						self.makeAssignment(i,j, d)
						hidden = True

		return hidden

	def pairs(self):
		hidden = False
		for i in range(9):
			for j in range(9):
				#determine square
				if i < 3:
					squareRow = 0
				elif i < 6:
					squareRow = 3
				else:
					squareRow = 6
				if j < 3:
					squareCol = 0
				elif j < 6:
					squareCol = 3
				else:
					squareCol = 6

				for d in range(0, len(self.domain[i][j]) - 1):
					control = False
					for z in range(d + 1, len(self.domain[i][j])):
						common = []

						count = 0
						if control == False:
							for x in range(9):
								if self.domain[i][j][d] in self.domain[i][x] and self.domain[i][j][z] in self.domain[i][x] and x != j:
									count += 1
								if self.domain[i][j][d] in self.domain[x][j] and self.domain[i][j][z] in self.domain[x][j] and x != i:
									count += 1
							for x in range(3):
								for y in range(3):
									if len(self.domain) > 2 and self.domain[i][j][d] in self.domain[x + squareRow][y + squareCol] and self.domain[i][j][z] in self.domain[x + squareRow][y + squareCol] and (x != i and y != j):
										count += 1

							if count < 2:
								common.append([i, x, self.domain[i][j][d], self.domain[i][j][z]])
							else:
								common = []
								control = False
							if count == 1:
								control = True

							count = 0
							if control:
								if len(self.domain[i][j]) > 2 or len(self.domain[common[0][0]][common[0][1]]) > 2:
									hidden = True
								self.domain[i][j] = [self.domain[i][j][d], self.domain[i][j][z]]
								self.domain[common[0][0]][common[0][1]] = [self.domain[i][j][0], self.domain[i][j][1]]
							common = []
		print(control)
		return hidden

	def triples(self):
		hidden = False
		for i in range(9):
			for j in range(9):
				#determine square
				if i < 3:
					squareRow = 0
				elif i < 6:
					squareRow = 3
				else:
					squareRow = 6
				if j < 3:
					squareCol = 0
				elif j < 6:
					squareCol = 3
				else:
					squareCol = 6

				for d in range(0, len(self.domain[i][j]) - 1):
					control = False
					for z in range(d + 1, len(self.domain[i][j])):
						common = []

						count = 0
						if control == False:
							for x in range(9):
								if self.domain[i][j][d] in self.domain[i][x] and self.domain[i][j][z] in self.domain[i][x] and x != j:
									count += 1
								if self.domain[i][j][d] in self.domain[x][j] and self.domain[i][j][z] in self.domain[x][j] and x != i:
									count += 1
							for x in range(3):
								for y in range(3):
									if len(self.domain) > 3 and self.domain[i][j][d] in self.domain[x + squareRow][y + squareCol] and self.domain[i][j][z] in self.domain[x + squareRow][y + squareCol] and (x != i and y != j):
										count += 1

							if count < 3:
								common.append([i, x, self.domain[i][j][d], self.domain[i][j][z]])
							else:
								common = []
								control = False
							if count == 2:
								control = True

							count = 0
							if control:
								if len(self.domain[i][j]) > 3 or len(self.domain[common[0][0]][common[0][1]]) > 3:
									hidden = True
								self.domain[i][j] = [self.domain[i][j][d], self.domain[i][j][z]]
								self.domain[common[0][0]][common[0][1]] = [self.domain[i][j][0], self.domain[i][j][1]]
							common = []
		print(control)
		return hidden

	def inference(self):
		control = False
		while control == False:
			nakedS = self.nakedSingle()
			hiddenS = self.hiddenSingle()
			if hiddenS == False:
				pair = self.pairs()
				if pair == False:
					#triple = self.triples()
					#if triple == False:
					control = True



def backtrackSearch(assign, board, varRow, varCol, MCV = False):
	step = 0
	if assign.assignmentComplete():
		return assignment

	for i in assign.domain[varRow][varCol]:
		tempAssign = assign
		tempAssign.makeAssignment(varRow, varCol, i)


easy, medium, hard, evil = readPuzzleFile("sudoku-problems.txt")
assign = assignment(evil[1])
assign.pairs()
#print("XXX")
assign.inference()
print(assign.board)

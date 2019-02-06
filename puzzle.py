#This program implements the RDFS and IDA* to solve a 15 puzzle
import random
import math
import numpy as np


compass = ["up","down","left","right"]
direction = {
	"up": (-1, 0),
	"down": (1, 0),
	"right": (0, 1),
	"left": (0, -1)
}

	#create a 4x4 grid with values 0 to 15 randomly assigned (0 is considered the empty space)
def createBoard():
	board = np.zeros((4,4))
	count = 1
	for i in range(4):
		for j in range(4):
			board[i][j] = count
			count += 1
	board[3][3] = 0
	return board

#returns a board position for a given a move
def move(board, dir):
	boardCopy = np.copy(board)
	position = np.where(board == 0)
	#check for illegal moves
	if position[0][0] == 0 and dir == "up":
		return []
	if position[0][0] == 3 and dir == "down":
		return []
	if position[1][0] == 0 and dir == "left":
		return []
	if position[1][0] == 3 and dir == "right":
		return []
	move = [position[0][0] + direction[dir][0], position[1][0] + direction[dir][1]]
	boardCopy[position[0][0]][position[1][0]] = boardCopy[move[0]][move[1]]
	boardCopy[move[0]][move[1]] = 0
	return boardCopy

#checks for sucessful board state
def goalTest(board):
	count = 1
	for i in range(4):
		for j in range(4):
			if i == 3 and j == 3:
				return True
			elif board[i][j] != count:
				return False
			count += 1

#Performs a series of m random moves where m is an inputted integer value
def scramble(board, m):
	compass = ["up", "down", "left", "right"]
	for i in range(m):
		control = False
		while control == False:
			r = random.randint(0,3)
			if move(board, compass[r]) != []:
				board = move(board, compass[r])
				control = True
				#print(board)
	return board

#measures a manhattan heuristic	of a given board position
def manhattan(board):
	h = 0
	for i in range(15):
		col = 0
		row = 0
		loc = np.where(board == i + 1)
		for j in range(i):
			col += 1
			if col == 4:
				col = 0
				row += 1
		h += abs(loc[0] - row) + abs(loc[1] - col)
	return h

#initiates an IDA* search
def IDA(root):
	bound = manhattan(root)
	path = [root]
	t = ""
	while t != "found":
		#begins a recursive search
		t = IDASearch(path, 0, bound)
		#increases the bound, allowing the search to go deeper
		bound = t
	return t

#performs a recurseive search using a manhattan heuristic
def IDASearch(path, g, bound):
	root = path[-1]
	f = g + manhattan(root)

	#print(f)
	if f > bound:
		return f
	if goalTest(root):
		return "found"

	#selects legal choices
	choice = []
	min = math.inf
	for i in range(4):
		if move(root, compass[i]) != []:
			choice.append(move(root, compass[i]))
	#print(choice)
	for i in choice:
		control = False
		#checks if configuration has a previously reached state, and culls it from the list of options if so
		for j in range(len(path)):
			if np.array_equal(j,path[j]):
				control = True
		if control == False:

			#print(root)
			path.append(i)
			t = IDASearch(path, g + 1, bound)
			if t == "found":
				return "found"
			elif t < min:
				min = t
			del path[-1]
	return min

#performs a recursive best first search
#3114
def RBFS(board, fLimit):
	fn = manhattan(board)
	if goalTest(board):
		return "success", board
	choice = []
	for i in range(4):
		if move(board, compass[i]) != []:
			moveCost = manhattan((move(board, compass[i])))
			choice.append((move(board, compass[i]), max(moveCost,fn)))

	succ = [choice[0]]
	for i in range(1, len(choice)):
		#print(i)
		for j in range(len(succ)):
			if manhattan(choice[i][0]) < manhattan(succ[j][0]):
				succ.insert(j,choice[i])
			elif j == len(succ) - 1:
				succ.append(choice[i])

	control = True
	while control == True:
		#print(succ)
		best = succ[0][0]
		#print(best)
		alt = succ[1][0]
		if manhattan(best) > fLimit:
			return "Fail", best
		result, best = RBFS(best, min(fLimit, manhattan(alt)))
		if result == "success":
			return result, best
			control = False



b1 = createBoard()
b1 = scramble(b1, 20)
b2 = b1
#print(IDA(b1))
print(RBFS(b2, 50))

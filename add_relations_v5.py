#!/usr/bin/env python3
"""Add RELATIONS by finding titles and inserting after EXPECTED_BEHAVIOR."""
import json

def load_mappings(mapping_file):
    with open(mapping_file, 'r') as f:
        return json.load(f)

def process_file(filepath, mappings):
    """Find test cases by title and add RELATIONS after EXPECTED_BEHAVIOR."""
    # Create title -> uid mapping
    title_to_rq = {item['test_case_title']: item['requirement_uids'] 
                   for item in mappings}
    
    with open(filepath, 'r') as f:
        lines = f.readlines()
    
    output_lines = []
    i = 0
    
    while i < len(lines):
        line = lines[i]
        output_lines.append(line)
        
        # Check if this is a test case TITLE
        if line.startswith('TITLE:') and i > 100:  # Avoid doc header
            title = line[6:].strip()
            
            # Check if this title matches any in mappings
            if title in title_to_rq:
                # Find the closing <<< of EXPECTED_BEHAVIOR for this test case
                j = i + 1
                last_closing = -1
                found_statement = False
                found_expected = False
                
                while j < len(lines) and (j < i + 200):  # Reasonable limit
                    curr_line = lines[j]
                    output_lines.append(curr_line)
                    
                    if 'STATEMENT:' in curr_line:
                        found_statement = True
                    
                    if 'EXPECTED_BEHAVIOR:' in curr_line:
                        found_expected = True
                    
                    # Look for closing <<<
                    if found_statement and curr_line.strip() == '<<<':
                        # This closes EXPECTED_BEHAVIOR
                        if found_expected or 'EXPECTED_BEHAVIOR' not in output_lines[-20:][0]:
                            last_closing = len(output_lines) - 1
                        # Stop after finding closing
                        break
                    
                    j += 1
                
                # If we found a closing bracket, insert RELATIONS
                if last_closing >= 0:
                    # Check next non-empty line
                    k = i + 1
                    while k < len(lines) and k < i + 200:
                        if lines[k].strip() and not lines[k].startswith('RELATIONS'):
                            # Peek ahead to see if RELATIONS already exists
                            peek = k
                            has_relations = False
                            while peek < len(lines) and peek < k + 10:
                                if 'RELATIONS:' in lines[peek]:
                                    has_relations = True
                                    break
                                if lines[peek].startswith('TITLE:'):
                                    break
                                peek += 1
                            
                            if not has_relations:
                                # Insert after the <<<
                                rel_lines = ['RELATIONS:\n']
                                for rq_uid in title_to_rq[title]:
                                    rel_lines.append('- TYPE: Parent\n')
                                    rel_lines.append(f'  VALUE: {rq_uid}\n')
                                    rel_lines.append('  ROLE: Verifies\n')
                                output_lines.extend(rel_lines)
                            break
                        k += 1
                
                i = j
                continue
        
        i += 1
    
    return ''.join(output_lines)

def main():
    import subprocess
    
    mapping_file = '/home/runner/work/gentlebeam-sk/gentlebeam-sk/tc_rq_mappings.json'
    mappings = load_mappings(mapping_file)
    
    internal_file = '/home/runner/work/gentlebeam-sk/gentlebeam-sk/cnc/voxelray-gentlebeamcnc/docs/internal/main_test_protocol.sdoc'
    
    # Restore
    subprocess.run(['git', 'checkout', internal_file], 
                   cwd='/home/runner/work/gentlebeam-sk/gentlebeam-sk', 
                   capture_output=True)
    
    print("Processing internal file...")
    modified = process_file(internal_file, mappings['internal'])
    
    with open(internal_file, 'w') as f:
        f.write(modified)
    
    count = modified.count('RELATIONS:')
    print(f"✓ Added {count} RELATIONS sections")

if __name__ == '__main__':
    main()

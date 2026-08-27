#!/usr/bin/env python3
"""Add RELATIONS to both external and internal test protocols."""
import json

def load_mappings(mapping_file):
    """Load TC to RQ mappings."""
    with open(mapping_file, 'r') as f:
        return json.load(f)

def add_relations_to_file(filepath, mappings):
    """Add RELATIONS to test cases using [TEST_CASE] blocks."""
    # Create mapping of test_case_uid -> requirement_uids
    tc_to_rq = {item['test_case_uid']: item['requirement_uids'] for item in mappings}
    
    with open(filepath, 'r') as f:
        lines = f.readlines()
    
    output_lines = []
    i = 0
    
    while i < len(lines):
        line = lines[i]
        
        # Check if this is a TEST_CASE start
        if line.strip() == '[TEST_CASE]':
            # Add the [TEST_CASE] line
            output_lines.append(line)
            i += 1
            
            # Get the UID
            current_uid = None
            if i < len(lines) and lines[i].startswith('UID:'):
                current_uid = lines[i].split(':', 1)[1].strip()
                output_lines.append(lines[i])
                i += 1
            
            # Add all lines until we hit another [TEST_CASE] or [[/SECTION]]
            last_closing_idx = -1
            while i < len(lines):
                curr_line = lines[i]
                output_lines.append(curr_line)
                
                # Track closing brackets
                if curr_line.strip() == '<<<':
                    last_closing_idx = len(output_lines) - 1
                
                # Check if we've hit the end of this test case
                if (curr_line.strip() == '[TEST_CASE]' or 
                    curr_line.strip().startswith('[[/SECTION]]')):
                    # Before moving to next, insert RELATIONS if needed
                    if (current_uid and current_uid in tc_to_rq and 
                        last_closing_idx >= 0):
                        # Check if RELATIONS already exists
                        has_relations = False
                        for j in range(last_closing_idx, len(output_lines)):
                            if 'RELATIONS:' in output_lines[j]:
                                has_relations = True
                                break
                        
                        if not has_relations:
                            # Insert RELATIONS before current line
                            rel_lines = ['RELATIONS:\n']
                            for rq_uid in tc_to_rq[current_uid]:
                                rel_lines.append('- TYPE: Parent\n')
                                rel_lines.append(f'  VALUE: {rq_uid}\n')
                                rel_lines.append('  ROLE: Verifies\n')
                            output_lines.extend(rel_lines)
                    
                    # Don't consume this line, process it in next iteration
                    break
                
                i += 1
            
            continue
        
        output_lines.append(line)
        i += 1
    
    return ''.join(output_lines)

def main():
    mapping_file = '/home/runner/work/gentlebeam-sk/gentlebeam-sk/tc_rq_mappings.json'
    mappings = load_mappings(mapping_file)
    
    # Process external (skip since already done)
    # external_file = '/home/runner/work/gentlebeam-sk/gentlebeam-sk/cnc/voxelray-gentlebeamcnc/docs/external/main_test_protocol.sdoc'
    # modified = add_relations_to_file(external_file, mappings['external'])
    # with open(external_file, 'w') as f:
    #     f.write(modified)
    # print(f"✓ Updated external: {modified.count('RELATIONS:')} RELATIONS")
    
    # Process internal
    internal_file = '/home/runner/work/gentlebeam-sk/gentlebeam-sk/cnc/voxelray-gentlebeamcnc/docs/internal/main_test_protocol.sdoc'
    print("Processing internal...")
    modified = add_relations_to_file(internal_file, mappings['internal'])
    with open(internal_file, 'w') as f:
        f.write(modified)
    print(f"✓ Updated internal: {modified.count('RELATIONS:')} RELATIONS")

if __name__ == '__main__':
    main()
